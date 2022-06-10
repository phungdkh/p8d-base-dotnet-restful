namespace P8D.Api.Services.Accounts.Handler
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using P8D.Domain.Entities;
    using P8D.Infrastructure.Common.Models;
    using P8D.Infrastructure.Common.Constants;
    using P8D.Infrastructure.Helpes;
    using P8D.Infrastructure.Services;
    using SendGrid.Helpers.Mail;
    using System.Linq;
    using P8D.Domain.Entities.Contexts;
    using P8D.Api.Services.Accounts.Request;

    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordHandler(AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _db = dbContext;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<ResponseModel> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = AppMessageConstants.USER_NOT_FOUND
                };


            var emailExpired = _db.ResetPasswordTokens.FirstOrDefault(x => x.Email == request.Email && x.ExpiredTime > DateTime.Now);
            var resetPasswordCode = emailExpired != null ? emailExpired.ResetPasswordCode : StringHelper.GenerateOTPNumber(6);

            var siteUrl = _configuration["SendingEmail:SiteUrl"];
            string resetPasswordPath = siteUrl + "reset-password/otp=" + resetPasswordCode;

            var templatePath = _configuration["SendingEmail:ResetPasswordPath"];
            string template = System.IO.File.ReadAllText(templatePath);

            string body = template.Replace("{EMAIL}", request.Email).Replace("{ResetPasswordPath}", resetPasswordPath);
            var FromEmail = _configuration["SendingEmail:FromEmail"];

            var emailMessageModel = new EmailMessageModel(new List<EmailAddress>()
                {
                    new EmailAddress(request.Email, FromEmail)
                }, "Forgot Password", body, null);

            if (emailExpired != null)
            {
                await _emailSender.SendEmailAsync(emailMessageModel);
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "An email has been sent again, please check email box."
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var otpToken = new ResetPasswordToken()
            {
                Email = request.Email,
                ResetPasswordCode = resetPasswordCode,
                Token = token,
                ExpiredTime = DateTime.Now.AddHours(Convert.ToDouble(_configuration.GetSection("AppSettings")["OTPTokenExpiredHours"]))
            };

            _db.Add(otpToken);
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(emailMessageModel);

            return new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Data = "Email has been sent"
            };
        }
    }
}
