using GuestFlow.Application.Models.Responses.Accounting;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Linq;

namespace GuestFlow.Application.Operations.Accounting
{
    public class JournalService : IJournalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public JournalService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
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
                    Description = "Accounts Receivable"
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
                        Description = $"{it.ServiceType} #{it.ServiceId}"
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
                        Description = "VAT Payable"
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
                            Description = "Adjustment / Rounding"
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
                            Description = "Adjustment / Rounding"
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

                var journal = new JournalEntry
                {
                    InvoiceId = request.InvoiceId,
                    PostingDate = postingDate,
                    Description = $"Posted for Invoice {invoice.InvoiceNumber}",
                    Currency = invoice.Currency ?? "USD",
                    CreatedBy = "system"
                };

                decimal totalDebit = 0, totalCredit = 0;
                foreach (var line in request.Lines)
                {
                    var jl = new JournalLine
                    {
                        AccountCode = line.AccountCode,
                        Debit = line.Debit,
                        Credit = line.Credit,
                        Description = line.Description,
                        ReferenceId = request.InvoiceId
                    };
                    journal.Lines.Add(jl);
                    totalDebit += line.Debit;
                    totalCredit += line.Credit;
                }

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
                    Lines = journal.Lines
                        .Select(l => new JournalLineDto
                        {
                            AccountCode = l.AccountCode,
                            Debit = l.Debit,
                            Credit = l.Credit,
                            Description = l.Description
                        })
                        .ToList()
                };

                return ApiResponse<JournalEntryResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<JournalEntryResponse>.Fail($"Failed to load journal: {ex.Message}");
            }
        }
    }
}

