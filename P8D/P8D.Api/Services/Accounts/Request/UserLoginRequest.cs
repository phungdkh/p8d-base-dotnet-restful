namespace P8D.Api.Services.Accounts.Request
{
    using System.ComponentModel.DataAnnotations;
    using MediatR;
    using P8D.Infrastructure.Common.Models;

    public class UserLoginRequest : IRequest<ResponseModel>
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
