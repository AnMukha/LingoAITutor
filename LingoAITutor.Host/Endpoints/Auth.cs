using LingoAITutor.Host.Dto;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            application.MapPost("api/login", Login).WithOpenApi(operation => new(operation)
            {
                Summary = "Login"                
            });            
        }

        private static async Task<IResult> Login(LingoDbContext context, LoginDto loginData)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == loginData.UserName);
            if (user == null)
            {
                return Results.Ok(new LoginResponse()
                {
                    Message = "User not found",
                    Token = null,
                    UserName = null
                });
            }

            var hasher = new PasswordHasher<User>();
            
            var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, loginData.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Results.Ok(new LoginResponse()
                {
                    Message = "Wrong password",
                    Token = null,
                    UserName = null
                });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("SECRET_KEY1SECRET_KEY1SECRET_KEY1SECRET_KEY1");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Results.Ok(new LoginResponse()
            {
                Message = null,
                Token = tokenHandler.WriteToken(token),
                UserName = loginData.UserName
            });
        }

    }
}
