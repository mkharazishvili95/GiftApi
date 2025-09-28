using FluentValidation;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GiftApi.Application.User.Commands.Login
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        readonly ApplicationDbContext _db;
        readonly IValidator<LoginUserCommand> _validator;
        readonly IConfiguration _configuration;

        public LoginUserHandler(ApplicationDbContext db, IValidator<LoginUserCommand> validator, IConfiguration configuration)
        {
            _db = db;
            _validator = validator;
            _configuration = configuration;
        }

        public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return new LoginUserResponse
                {
                    Success = false,
                    UserMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    StatusCode = 400
                };
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName.ToUpper() == request.UserName.ToUpper(), cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return new LoginUserResponse
                {
                    Success = false,
                    UserMessage = "Incorrect username or password.",
                    StatusCode = 401
                };
            }

            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Type.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            _db.Users.Update(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new LoginUserResponse
            {
                Success = true,
                UserMessage = "Login successful",
                StatusCode = 200,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
