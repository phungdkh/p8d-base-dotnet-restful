using System.ComponentModel.DataAnnotations;
using MediatR;
using P8D.Infrastructure.Common.Models;

namespace P8D.Api.Services.Accounts.Request
{
    public class ResetPasswordRequest : IRequest<ResponseModel>
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }

        public string ResetPasswordCode { get; set; }
    }
}
