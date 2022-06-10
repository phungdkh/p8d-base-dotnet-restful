using MediatR;
using Newtonsoft.Json;
using P8D.Infrastructure.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace P8D.Api.Services.Accounts.Request
{
    public class VerifyTokenRequest : IRequest<ResponseModel>
    {
        [Required]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
