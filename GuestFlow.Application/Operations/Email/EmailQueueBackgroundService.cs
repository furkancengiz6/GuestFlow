using GuestFlow.Application.Operations.Email.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public class EmailQueueBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailQueueBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

        public EmailQueueBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<EmailQueueBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("E-posta kuyruk servisi başlatıldı.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailQueueService = scope.ServiceProvider.GetRequiredService<IEmailQueueService>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var emailHistoryService = scope.ServiceProvider.GetRequiredService<IEmailHistoryService>();
                        var emailTemplateService = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();

                        // Bekleyen e-postayı al
                        var nextEmailResult = await emailQueueService.GetNextPendingEmailAsync();
                        if (nextEmailResult.IsSuccess && nextEmailResult.Data != null)
                        {
                            var emailQueue = nextEmailResult.Data;
                            await ProcessEmailAsync(emailQueue, emailService, emailQueueService, emailHistoryService, emailTemplateService);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "E-posta kuyruk işleme sırasında hata oluştu.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("E-posta kuyruk servisi durduruldu.");
        }

        private async Task ProcessEmailAsync(
            Dtos.EmailQueueDto emailQueue,
            IEmailService emailService,
            IEmailQueueService emailQueueService,
            IEmailHistoryService emailHistoryService,
            IEmailTemplateService emailTemplateService)
        {
            try
            {
                string subject = emailQueue.Subject;
                string body = emailQueue.Body;

                // Şablon kullanılıyorsa render et
                if (!string.IsNullOrEmpty(emailQueue.TemplateName) && emailQueue.TemplateVariables != null)
                {
                    var templateResult = await emailTemplateService.RenderTemplateAsync(
                        emailQueue.TemplateName,
                        emailQueue.TemplateVariables);

                    if (templateResult.IsSuccess && !string.IsNullOrEmpty(templateResult.Data))
                    {
                        body = templateResult.Data;

                        // Şablonun subject'ini de al
                        var templateInfo = await emailTemplateService.GetTemplateByNameAsync(emailQueue.TemplateName);
                        if (templateInfo.IsSuccess && templateInfo.Data != null)
                        {
                            subject = RenderTemplateString(templateInfo.Data.Subject, emailQueue.TemplateVariables);
                        }
                    }
                }

                // E-postayı gönder
                var attachments = emailQueue.Attachments;
                var sendResult = await emailService.SendEmailAsync(
                    emailQueue.To,
                    subject,
                    body,
                    emailQueue.IsHtml,
                    attachments);

                // Geçmişe kaydet
                await emailHistoryService.SaveEmailHistoryAsync(
                    emailQueue.To,
                    "noreply@guestflow.com", // From email
                    subject,
                    sendResult ? "Sent" : "Failed",
                    sendResult ? null : "E-posta gönderilemedi",
                    emailQueue.TemplateName,
                    emailQueue.RelatedEntityType,
                    emailQueue.RelatedEntityId);

                // Kuyruk durumunu güncelle
                if (sendResult)
                {
                    await emailQueueService.UpdateQueueStatusAsync(emailQueue.Id, "Sent");
                    _logger.LogInformation($"E-posta başarıyla gönderildi: {emailQueue.To}");
                }
                else
                {
                    await emailQueueService.UpdateQueueStatusAsync(
                        emailQueue.Id,
                        emailQueue.RetryCount >= emailQueue.MaxRetryCount ? "Failed" : "Pending",
                        "E-posta gönderilemedi");
                    _logger.LogWarning($"E-posta gönderilemedi: {emailQueue.To}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta işlenirken hata: {ex.Message}");
                await emailQueueService.UpdateQueueStatusAsync(
                    emailQueue.Id,
                    emailQueue.RetryCount >= emailQueue.MaxRetryCount ? "Failed" : "Pending",
                    ex.Message);
            }
        }

        private string RenderTemplateString(string template, Dictionary<string, string>? variables)
        {
            if (variables == null || variables.Count == 0)
                return template;

            var result = template;
            foreach (var variable in variables)
            {
                var pattern = $"{{{{ {variable.Key} }}}}";
                result = result.Replace(pattern, variable.Value);
            }

            return result;
        }
    }
}

