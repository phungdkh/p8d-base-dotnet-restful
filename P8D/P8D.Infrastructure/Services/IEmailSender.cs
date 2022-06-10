using Microsoft.Extensions.Configuration;
using P8D.Infrastructure.Common.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace P8D.Infrastructure.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(EmailMessageModel emailMessageModel);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration Configuration;

        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task SendEmailAsync(EmailMessageModel emailMessageModel)
        {
            var apiKey = Configuration["SendingEmail:SendGridApiKey"];
            var fromEmail = Configuration["SendingEmail:FromEmail"];
            var fromName = Configuration["SendingEmail:FromName"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, emailMessageModel.Recipients, emailMessageModel.Subject, emailMessageModel.Content, emailMessageModel.Content);
            if (emailMessageModel.Files != null)
            {
                msg.Attachments = new List<Attachment>();
                foreach (var file in emailMessageModel.Files)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            string base64 = Convert.ToBase64String(fileBytes);
                            var att = new Attachment()
                            {
                                Filename = file.FileName,
                                Content = base64,
                                Type = file.ContentType
                            };
                            msg.Attachments.Add(att);
                        }
                    }
                }
            }
            await client.SendEmailAsync(msg);
        }
    }
}
