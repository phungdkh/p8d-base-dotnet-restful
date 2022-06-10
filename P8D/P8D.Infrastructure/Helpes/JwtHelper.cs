using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace P8D.Infrastructure.Helpes
{
    public static class JwtHelper
    {
        public static object GenerateJwtToken(List<Claim> claims, IConfiguration _configuration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings")["ThirdPartyRelationshipSecret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration.GetSection("AppSettings")["TokenExpiredMinutes"]));

            var token = new JwtSecurityToken(
                _configuration.GetSection("AppSettings")["TokenIssuer"],
                _configuration.GetSection("AppSettings")["TokenAudience"],
                claims,
                null,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static List<Claim> GenerateClaims(string claimType, List<string> claims)
        {
            var res = new List<Claim>();
            foreach (var claim in claims)
            {
                res.Add(new Claim(claimType, claim));
            }

            return res;
        }

        public static bool ValidateJwtToken(string token, IConfiguration _configuration)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings")["ThirdPartyRelationshipSecret"])),
                ValidAudience = _configuration.GetSection("AppSettings")["TokenAudience"],
                ValidIssuer = _configuration.GetSection("AppSettings")["TokenIssuer"],
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
