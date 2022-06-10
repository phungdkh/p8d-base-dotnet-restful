namespace P8D.Api.Services.Accounts.Handler
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using P8D.Infrastructure.Helpes;
    using P8D.Infrastructure.Common.Models;
    using P8D.Api.Services.Accounts.Request;

    public class VerifyTokenHandler : IRequestHandler<VerifyTokenRequest, ResponseModel>
    {
        private readonly IConfiguration _configuration;

        public VerifyTokenHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ResponseModel> Handle(VerifyTokenRequest request, CancellationToken cancellationToken)
        {
            var result = await Task.Run(() => JwtHelper.ValidateJwtToken(request.AccessToken, _configuration));
            if (result == true)
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = request
                };
            }
            else
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Token is invalid"
                };
            }
        }
    }
}
