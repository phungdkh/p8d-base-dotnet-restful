using System.ComponentModel.DataAnnotations;
using MediatR;
using P8D.Infrastructure.Common.Models;

namespace P8D.Api.Services.Accounts.Request
{
    public class ForgotPasswordRequest : IRequest<ResponseModel>
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
