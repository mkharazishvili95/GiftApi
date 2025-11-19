using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.User.Commands.Auth.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResponse>
    {
        readonly IUserRepository _users;
        public ResetPasswordHandler(IUserRepository users)
        {
            _users = users;
        }

        public async Task<ResetPasswordResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return new ResetPasswordResponse { Success = false, StatusCode = 400, Message = "Token required" };
            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return new ResetPasswordResponse { Success = false, StatusCode = 400, Message = "New password required" };
            if (request.NewPassword != request.ConfirmPassword)
                return new ResetPasswordResponse { Success = false, StatusCode = 400, Message = "Passwords do not match" };

            var tokenEntity = await _users.GetPasswordResetTokenAsync(request.Token);
            if (tokenEntity == null || tokenEntity.Used || tokenEntity.ExpiresUtc < DateTime.UtcNow)
                return new ResetPasswordResponse { Success = false, StatusCode = 400, Message = "Invalid or expired token" };

            var user = await _users.GetByIdAsync(tokenEntity.UserId);
            if (user == null)
                return new ResetPasswordResponse { Success = false, StatusCode = 404, Message = "User not found" };

            string hashed = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _users.UpdatePasswordAsync(user, hashed);
            await _users.MarkPasswordResetTokenUsedAsync(tokenEntity);

            return new ResetPasswordResponse { Success = true, StatusCode = 200, Message = "Password reset successful" };
        }
    }
}