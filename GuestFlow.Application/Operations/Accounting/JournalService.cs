using GuestFlow.Application.Models.Responses.Accounting;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

namespace GuestFlow.Application.Operations.Accounting
{
    public class JournalService : IJournalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GuestFlow.Application.Operations.Currency.IExchangeRateService _exchangeRateService;

        public JournalService(
            IUnitOfWork unitOfWork,
            IRepository<PersonnelEntity> personnelRepository,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            GuestFlow.Application.Operations.Currency.IExchangeRateService exchangeRateService)
        {
            _unitOfWork = unitOfWork;
            _personnelRepository = personnelRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _exchangeRateService = exchangeRateService;
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        private string? GetCurrentUserId()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.User == null) return null;

                return httpContext.User.FindFirst("id")?.Value ??
                       httpContext.User.FindFirst("sub")?.Value ??
                       httpContext.User.FindFirst("userId")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get current user name from claims
        /// </summary>
        private string? GetCurrentUserName()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext?.User == null) return null;

                return httpContext.User.FindFirst("FullName")?.Value ??
                       httpContext.User.FindFirst("name")?.Value ??
                       httpContext.User.Identity?.Name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get current user's FullName from Personnel entity (more reliable than claims)
        /// </summary>
        private async Task<string?> GetCurrentUserFullNameFromPersonnelAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int personnelId))
                    return null;

                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                return personnel?.FullName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Round amount according to currency rounding rules
        /// </summary>
        private decimal RoundAmount(decimal amount, string currency)
        {
            // Default rounding: 2 decimal places
            var decimals = 2;

            // Currency-specific rounding rules
            switch (currency.ToUpperInvariant())
            {
                case "JPY": // Japanese Yen: no decimals
                case "KRW": // Korean Won: no decimals
                    decimals = 0;
                    break;
                case "BHD": // Bahraini Dinar: 3 decimals
                case "JOD": // Jordanian Dinar: 3 decimals
                case "KWD": // Kuwaiti Dinar: 3 decimals
                case "OMR": // Omani Rial: 3 decimals
                case "TND": // Tunisian Dinar: 3 decimals
                    decimals = 3;
                    break;
                default:
                    decimals = 2; // Most currencies: 2 decimals
                    break;
            }

            return Math.Round(amount, decimals, MidpointRounding.AwayFromZero);
        }

        private string GetReceivableAccountCode()
            => _configuration["Accounting:Journal:ReceivableAccountCode"] ?? "1100";

        private string GetAdjustmentAccountCode()
            => _configuration["Accounting:Journal:AdjustmentAccountCode"] ?? "9999";

        private string GetDefaultRevenueAccountCode()
            => _configuration["Accounting:Journal:DefaultRevenueAccountCode"] ?? "4000";

        private string GetVatPayableAccountCode()
            => _configuration["Accounting:Journal:VatPayableAccountCode"] ?? "3910";

        private static decimal SafeNetFromGrossAndVat(decimal gross, decimal vatAmount)
        {
            var net = gross - vatAmount;
            return net < 0m ? 0m : net;
        }

        private string GetRevenueAccountCodeForServiceType(string? serviceType)
        {
            var key = (serviceType ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(key)) return GetDefaultRevenueAccountCode();

            // Try as-is and lowercased key for config JSON flexibility.
            var byType = _configuration[$"Accounting:Journal:RevenueAccountByServiceType:{key}"]
                         ?? _configuration[$"Accounting:Journal:RevenueAccountByServiceType:{key.ToLowerInvariant()}"];

            return string.IsNullOrWhiteSpace(byType) ? GetDefaultRevenueAccountCode() : byType;
        }

        public async Task<ApiResponse<JournalPreviewResponse>> GenerateJournalPreviewAsync(int invoiceId)
        {
            try
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
                if (invoice == null) return ApiResponse<JournalPreviewResponse>.Fail("Invoice not found");

                var items = await _unitOfWork.InvoiceItems.GetAll(ii => ii.InvoiceId == invoiceId).ToListAsync();

                var preview = new JournalPreviewResponse
                {
                    InvoiceId = invoiceId,
                    Description = $"Invoice {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "USD"
                };

                // Simple mapping (VAT-inclusive item amounts):
                // - Debit receivable (gross)
                // - Credit revenue (net)
                // - Credit VAT payable (VAT portion)
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                // Receivable line
                var sumGross = items.Sum(i => i.Amount);
                var receivableAmount = invoice.TotalAmount > 0m ? invoice.TotalAmount : sumGross;
                preview.Lines.Add(new JournalLineDto
                {
                    AccountCode = GetReceivableAccountCode(),
                    Debit = receivableAmount,
                    Credit = 0,
                    Description = "Accounts Receivable",
                    Currency = preview.Currency,
                    ExchangeRate = null // Same currency as journal
                });
                totalDebit += receivableAmount;

                // Revenue lines per item
                decimal totalVat = 0m;
                foreach (var it in items)
                {
                    var net = SafeNetFromGrossAndVat(it.Amount, it.VatAmount);
                    if (it.VatAmount > 0m) totalVat += it.VatAmount;

                    preview.Lines.Add(new JournalLineDto
                    {
                        AccountCode = GetRevenueAccountCodeForServiceType(it.ServiceType),
                        Debit = 0,
                        Credit = net,
                        Description = $"{it.ServiceType} #{it.ServiceId}",
                        Currency = preview.Currency,
                        ExchangeRate = null // Same currency as journal
                    });
                    totalCredit += net;
                }

                // VAT payable (single summary line)
                if (totalVat > 0m)
                {
                    preview.Lines.Add(new JournalLineDto
                    {
                        AccountCode = GetVatPayableAccountCode(),
                        Debit = 0,
                        Credit = totalVat,
                        Description = "VAT Payable",
                        Currency = preview.Currency,
                        ExchangeRate = null // Same currency as journal
                    });
                    totalCredit += totalVat;
                }

                // If invoice total differs from sum of items (due to discounts/adjustments/rounding),
                // add an adjustment line so debits equal credits.
                var adjustment = receivableAmount - sumGross;
                if (adjustment != 0m)
                {
                    // Positive adjustment means receivable > items sum -> add credit adjustment
                    if (adjustment > 0m)
                    {
                        preview.Lines.Add(new JournalLineDto
                        {
                            AccountCode = GetAdjustmentAccountCode(),
                            Debit = 0,
                            Credit = adjustment,
                            Description = "Adjustment / Rounding",
                            Currency = preview.Currency,
                            ExchangeRate = null // Same currency as journal
                        });
                        totalCredit += adjustment;
                    }
                    else
                    {
                        // Negative adjustment -> extra debit required
                        preview.Lines.Add(new JournalLineDto
                        {
                            AccountCode = GetAdjustmentAccountCode(),
                            Debit = Math.Abs(adjustment),
                            Credit = 0,
                            Description = "Adjustment / Rounding",
                            Currency = preview.Currency,
                            ExchangeRate = null // Same currency as journal
                        });
                        totalDebit += Math.Abs(adjustment);
                    }
                }

                preview.TotalDebit = totalDebit;
                preview.TotalCredit = totalCredit;

                return ApiResponse<JournalPreviewResponse>.SuccessResponse(preview);
            }
            catch (Exception ex)
            {
                return ApiResponse<JournalPreviewResponse>.Fail($"Failed to generate preview: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> PostJournalAsync(JournalPostRequest request)
        {
            try
            {
                if (request == null)
                    return ApiResponse<bool>.Fail("Request is required");

                if (request.Lines == null || request.Lines.Count == 0)
                    return ApiResponse<bool>.Fail("Journal lines are required");

                // Idempotency guard: don't post twice for the same invoice.
                // (DB also enforces this via JournalEntry.InvoiceId unique index when not null.)
                var alreadyPosted = await _unitOfWork.JournalEntries
                    .GetAll(j => j.InvoiceId == request.InvoiceId)
                    .AnyAsync();

                if (alreadyPosted)
                    return ApiResponse<bool>.Fail("Journal already posted for this invoice");

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
                if (invoice == null) return ApiResponse<bool>.Fail("Invoice not found");

                if (!DateTime.TryParseExact(
                        request.PostingDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                        out var postingDate))
                    return ApiResponse<bool>.Fail("Invalid posting date. Expected format: yyyy-MM-dd");

                var journalCurrency = invoice.Currency ?? "USD";
                var currentUserId = GetCurrentUserId();
                
                // Get PersonnelId and FullName from Personnel entity (hybrid approach: ID + Snapshot)
                int? currentUserPersonnelId = null;
                string? currentUserFullName = null;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out int personnelId))
                {
                    currentUserPersonnelId = personnelId;
                    var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                    currentUserFullName = personnel?.FullName ?? GetCurrentUserName() ?? "system";
                }
                else
                {
                    currentUserFullName = GetCurrentUserName() ?? "system";
                }

                var journal = new JournalEntry
                {
                    InvoiceId = request.InvoiceId,
                    PostingDate = postingDate,
                    Description = $"Posted for Invoice {invoice.InvoiceNumber}",
                    Currency = journalCurrency, // Journal entry base currency
                    // Hybrid approach: ID for referential integrity, Snapshot for historical accuracy
                    CreatedBy = currentUserFullName, // Snapshot: FullName at creation time
                    CreatedByPersonnelId = currentUserPersonnelId, // Foreign key: Personnel ID
                    PostedBy = currentUserFullName, // Snapshot: FullName at posting time
                    PostedByPersonnelId = currentUserPersonnelId, // Foreign key: Personnel ID
                    PostedDate = DateTime.UtcNow
                };

                decimal totalDebit = 0, totalCredit = 0;
                foreach (var line in request.Lines)
                {
                    // Get line currency (default to journal currency if not specified)
                    var lineCurrency = string.IsNullOrWhiteSpace(line.Currency)
                        ? journalCurrency
                        : line.Currency!.ToUpperInvariant();
                    
                    // Apply rounding rules in line currency
                    var roundedDebit = RoundAmount(line.Debit, lineCurrency);
                    var roundedCredit = RoundAmount(line.Credit, lineCurrency);

                    // Calculate exchange rate if currencies differ
                    decimal? exchangeRate = null;
                    decimal debitInJournal = roundedDebit;
                    decimal creditInJournal = roundedCredit;

                    if (!string.Equals(lineCurrency, journalCurrency, StringComparison.OrdinalIgnoreCase))
                    {
                        exchangeRate = await _exchangeRateService.GetExchangeRateAsync(lineCurrency, journalCurrency);
                        if (exchangeRate <= 0)
                            return ApiResponse<bool>.Fail($"Invalid exchange rate {lineCurrency}->{journalCurrency}");

                        debitInJournal = RoundAmount(roundedDebit * exchangeRate.Value, journalCurrency);
                        creditInJournal = RoundAmount(roundedCredit * exchangeRate.Value, journalCurrency);
                    }

                    var jl = new JournalLine
                    {
                        AccountCode = line.AccountCode,
                        Debit = debitInJournal,
                        Credit = creditInJournal,
                        Description = line.Description,
                        ReferenceId = request.InvoiceId,
                        Currency = lineCurrency, // Line currency
                        ExchangeRate = exchangeRate // Exchange rate to journal currency
                    };
                    journal.Lines.Add(jl);
                    totalDebit += debitInJournal;
                    totalCredit += creditInJournal;
                }

                // Final rounding check
                totalDebit = RoundAmount(totalDebit, journalCurrency);
                totalCredit = RoundAmount(totalCredit, journalCurrency);

                if (totalDebit != totalCredit)
                    return ApiResponse<bool>.Fail($"Journal is not balanced (debit={totalDebit}, credit={totalCredit})");

                journal.TotalDebit = totalDebit;
                journal.TotalCredit = totalCredit;

                await _unitOfWork.JournalEntries.AddAsync(journal);
                await _unitOfWork.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Journal posted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail($"Failed to post journal: {ex.Message}");
            }
        }

        public async Task<ApiResponse<JournalEntryResponse>> GetJournalByInvoiceAsync(int invoiceId)
        {
            try
            {
                var journal = await _unitOfWork.JournalEntries
                    .GetAll(j => j.InvoiceId == invoiceId, j => j.Lines)
                    .OrderByDescending(j => j.PostingDate)
                    .FirstOrDefaultAsync();

                if (journal == null)
                    return ApiResponse<JournalEntryResponse>.Fail("Journal not found for this invoice", 404);

                var response = new JournalEntryResponse
                {
                    JournalEntryId = journal.Id,
                    InvoiceId = journal.InvoiceId ?? invoiceId,
                    PostingDate = journal.PostingDate.ToString("yyyy-MM-dd"),
                    Description = journal.Description,
                    Currency = journal.Currency,
                    TotalDebit = journal.TotalDebit,
                    TotalCredit = journal.TotalCredit,
                    CreatedBy = journal.CreatedBy,
                    PostedBy = journal.PostedBy,
                    PostedDate = journal.PostedDate?.ToString("yyyy-MM-dd"),
                    Lines = journal.Lines
                        .Select(l => new JournalLineDto
                        {
                            AccountCode = l.AccountCode,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            Description = l.Description,
                            Currency = l.Currency,
                            ExchangeRate = l.ExchangeRate
                        })
                        .ToList()
                };

                return ApiResponse<JournalEntryResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<JournalEntryResponse>.Fail($"Failed to get journal: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a reversal entry for an existing journal entry.
        /// POLICY: Unpost is NOT allowed - only reversal entries can be created.
        /// This maintains audit trail integrity by keeping both original and reversal entries.
        /// </summary>
        public async Task<ApiResponse<JournalEntryResponse>> ReverseJournalEntryAsync(int journalEntryId, string? reversalDescription = null)
        {
            try
            {
                // Get the original journal entry
                var originalJournal = await _unitOfWork.JournalEntries
                    .GetAll(j => j.Id == journalEntryId, j => j.Lines)
                    .FirstOrDefaultAsync();

                if (originalJournal == null)
                    return ApiResponse<JournalEntryResponse>.Fail("Journal entry not found", 404);

                // Check if already reversed
                if (originalJournal.IsReversed)
                    return ApiResponse<JournalEntryResponse>.Fail("Journal entry has already been reversed", 400);

                // Check if reversal entry already exists
                var existingReversal = await _unitOfWork.JournalEntries
                    .GetAll(j => j.ReversedByJournalEntryId == journalEntryId)
                    .FirstOrDefaultAsync();

                if (existingReversal != null)
                    return ApiResponse<JournalEntryResponse>.Fail("A reversal entry already exists for this journal entry", 400);

                // Get PersonnelId and FullName from Personnel entity (hybrid approach: ID + Snapshot)
                var currentUserId = GetCurrentUserId();
                int? currentUserPersonnelId = null;
                string? currentUserFullName = null;
                
                if (!string.IsNullOrEmpty(currentUserId) && int.TryParse(currentUserId, out int personnelId))
                {
                    currentUserPersonnelId = personnelId;
                    var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                    currentUserFullName = personnel?.FullName ?? GetCurrentUserName() ?? "system";
                }
                else
                {
                    currentUserFullName = GetCurrentUserName() ?? "system";
                }
                
                var journalCurrency = originalJournal.Currency;

                // Create reversal entry (swap debits and credits)
                var reversalJournal = new JournalEntry
                {
                    InvoiceId = originalJournal.InvoiceId, // Keep same invoice reference
                    PostingDate = DateTime.UtcNow.Date,
                    Description = reversalDescription ?? $"Reversal of Journal Entry #{originalJournal.Id} - {originalJournal.Description}",
                    Currency = journalCurrency,
                    // Hybrid approach: ID for referential integrity, Snapshot for historical accuracy
                    CreatedBy = currentUserFullName, // Snapshot: FullName at creation time
                    CreatedByPersonnelId = currentUserPersonnelId, // Foreign key: Personnel ID
                    PostedBy = currentUserFullName, // Snapshot: FullName at posting time
                    PostedByPersonnelId = currentUserPersonnelId, // Foreign key: Personnel ID
                    PostedDate = DateTime.UtcNow,
                    // Mark this as a reversal entry
                    ReversedByJournalEntryId = journalEntryId
                };

                decimal totalDebit = 0, totalCredit = 0;

                // Create reversal lines (swap debits and credits)
                foreach (var originalLine in originalJournal.Lines)
                {
                    // Swap debit and credit for reversal
                    var reversedDebit = RoundAmount(originalLine.Credit, originalLine.Currency);
                    var reversedCredit = RoundAmount(originalLine.Debit, originalLine.Currency);

                    var reversalLine = new JournalLine
                    {
                        AccountCode = originalLine.AccountCode,
                        Debit = reversedDebit, // Original credit becomes debit
                        Credit = reversedCredit, // Original debit becomes credit
                        Description = $"Reversal: {originalLine.Description}",
                        ReferenceId = originalJournal.InvoiceId,
                        Currency = originalLine.Currency,
                        ExchangeRate = originalLine.ExchangeRate
                    };

                    reversalJournal.Lines.Add(reversalLine);
                    totalDebit += reversedDebit;
                    totalCredit += reversedCredit;
                }

                // Final rounding check
                totalDebit = RoundAmount(totalDebit, journalCurrency);
                totalCredit = RoundAmount(totalCredit, journalCurrency);

                if (totalDebit != totalCredit)
                    return ApiResponse<JournalEntryResponse>.Fail($"Reversal journal is not balanced (debit={totalDebit}, credit={totalCredit})");

                reversalJournal.TotalDebit = totalDebit;
                reversalJournal.TotalCredit = totalCredit;

                // Mark original entry as reversed
                originalJournal.IsReversed = true;
                originalJournal.ReversedBy = currentUserFullName; // Snapshot: FullName at reversal time
                originalJournal.ReversedByPersonnelId = currentUserPersonnelId; // Foreign key: Personnel ID
                originalJournal.ReversedDate = DateTime.UtcNow;

                // Save both entries
                await _unitOfWork.JournalEntries.AddAsync(reversalJournal);
                await _unitOfWork.JournalEntries.UpdateAsync(originalJournal);
                await _unitOfWork.CommitAsync();

                // Return the reversal entry response
                var response = new JournalEntryResponse
                {
                    JournalEntryId = reversalJournal.Id,
                    InvoiceId = reversalJournal.InvoiceId ?? 0,
                    PostingDate = reversalJournal.PostingDate.ToString("yyyy-MM-dd"),
                    Description = reversalJournal.Description,
                    Currency = reversalJournal.Currency,
                    TotalDebit = reversalJournal.TotalDebit,
                    TotalCredit = reversalJournal.TotalCredit,
                    CreatedBy = reversalJournal.CreatedBy,
                    PostedBy = reversalJournal.PostedBy,
                    PostedDate = reversalJournal.PostedDate?.ToString("yyyy-MM-dd"),
                    Lines = reversalJournal.Lines
                        .Select(l => new JournalLineDto
                        {
                            AccountCode = l.AccountCode,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            Description = l.Description,
                            Currency = l.Currency,
                            ExchangeRate = l.ExchangeRate
                        })
                        .ToList()
                };

                return ApiResponse<JournalEntryResponse>.SuccessResponse(response, "Journal entry reversed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<JournalEntryResponse>.Fail($"Failed to reverse journal entry: {ex.Message}");
            }
        }
    }
}

