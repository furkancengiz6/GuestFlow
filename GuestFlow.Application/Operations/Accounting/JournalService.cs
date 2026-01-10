using GuestFlow.Application.Models.Responses.Accounting;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GuestFlow.Application.Operations.Accounting
{
    public class JournalService : IJournalService
    {
        private readonly IUnitOfWork _unitOfWork;

        public JournalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                // Simple mapping: debit receivable, credit revenue and VAT
                decimal totalDebit = 0;
                decimal totalCredit = 0;

                // Receivable line
                var sumItems = items.Sum(i => i.Amount);
                var receivableAmount = invoice.TotalAmount > 0m ? invoice.TotalAmount : sumItems;
                preview.Lines.Add(new JournalLineDto
                {
                    AccountCode = "1100", // Receivables default - editable in GL mapping later
                    Debit = receivableAmount,
                    Credit = 0,
                    Description = "Accounts Receivable"
                });
                totalDebit += receivableAmount;

                // Revenue lines per item
                foreach (var it in items)
                {
                    preview.Lines.Add(new JournalLineDto
                    {
                        AccountCode = "4000", // Revenue default
                        Debit = 0,
                        Credit = it.Amount,
                        Description = $"{it.ServiceType} #{it.ServiceId}"
                    });
                    totalCredit += it.Amount;
                }

                // If invoice total differs from sum of items (due to discounts/adjustments/rounding),
                // add an adjustment line so debits equal credits.
                var adjustment = receivableAmount - sumItems;
                if (adjustment != 0m)
                {
                    // Positive adjustment means receivable > items sum -> add credit adjustment
                    if (adjustment > 0m)
                    {
                        preview.Lines.Add(new JournalLineDto
                        {
                            AccountCode = "9999", // Adjustment / Rounding account - configurable in GL mapping later
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
                            AccountCode = "9999",
                            Debit = Math.Abs(adjustment),
                            Credit = 0,
                            Description = "Adjustment / Rounding"
                        });
                        totalDebit += Math.Abs(adjustment);
                    }
                }

                // VAT handling: invoice does not store VAT as a single field; VAT (if any) should
                // be derived from invoice items. Currently InvoiceItemEntity has no VAT field,
                // so VAT lines are not generated here.

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
                // Idempotency guard: don't post twice for the same invoice.
                var alreadyPosted = await _unitOfWork.JournalLines
                    .GetAll(jl => jl.ReferenceId == request.InvoiceId)
                    .AnyAsync();

                if (alreadyPosted)
                    return ApiResponse<bool>.Fail("Journal already posted for this invoice");

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(request.InvoiceId);
                if (invoice == null) return ApiResponse<bool>.Fail("Invoice not found");

                var journal = new JournalEntry
                {
                    InvoiceId = request.InvoiceId,
                    PostingDate = DateTime.Parse(request.PostingDate),
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

