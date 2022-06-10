namespace P8D.Api.Services.Accounts.Handler
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using P8D.Domain.Entities;
    using P8D.Domain.Entities.Contexts;
    using P8D.Infrastructure.Helpes;
    using P8D.Infrastructure.Common.Models;
    using P8D.Api.Services.Accounts.Request;

    public class UserLoginHandler : IRequestHandler<UserLoginRequest, ResponseModel>
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public UserLoginHandler(AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _db = dbContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<ResponseModel> Handle(UserLoginRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email)
                   ?? await _userManager.FindByNameAsync(request.Email);

            var passwordIsCorrect = await _userManager.CheckPasswordAsync(user, request.Password);
            if (passwordIsCorrect)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name)
                };
                claims.AddRange(JwtHelper.GenerateClaims(ClaimTypes.Role, roles.ToList()));

                var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
                if (claims != null && claimsPrincipal?.Identity is ClaimsIdentity claimsIdentity)
                {
                    claimsIdentity.AddClaims(claims);
                    await _signInManager.SignInWithClaimsAsync(user,
                    true,
                    claimsIdentity.Claims);
                }

                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = new
                    {
                        access_token = JwtHelper.GenerateJwtToken(claims, _configuration)
                    }
                };
            }
            else
            {
                return new ResponseModel
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "User or Password is invalid"
                };
            }
        }
    }
}
