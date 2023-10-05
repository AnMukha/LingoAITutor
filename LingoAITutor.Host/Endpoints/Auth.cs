using Azure.Identity;
using LingoAITutor.Host.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LingoAITutor.Host.Endpoints
{
    public static class Auth
    {

        public static void AddEndpoints(WebApplication application)
        {
            application.MapGet("api/login", Login).WithOpenApi(operation => new(operation)
            {
                Summary = "Login",
            });            
        }

        private static IResult Login(string userName, string password)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SECRET_KEY1SECRET_KEY1SECRET_KEY1SECRET_KEY1");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userName) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Results.Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });

        }

    }
}
