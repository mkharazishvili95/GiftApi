using FluentValidation;
using GiftApi.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GiftApi.Application.Features.User.Commands.Login
{
    public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
    {
        private readonly IValidator<LoginUserCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public LoginUserHandler(
            IValidator<LoginUserCommand> validator,
            IUserRepository userRepository,
            IConfiguration configuration)
        {
            _validator = validator;
            _userRepository = userRepository;
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
                    Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                    StatusCode = 400
                };
            }

            var user = await _userRepository.GetByUserNameAsync(request.UserName);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return new LoginUserResponse
                {
                    Success = false,
                    Message = "Incorrect username or password.",
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

            await _userRepository.UpdateRefreshTokenAsync(user, refreshToken, DateTime.UtcNow.AddDays(7));

            return new LoginUserResponse
            {
                Success = true,
                Message = "Login successful",
                StatusCode = 200,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}