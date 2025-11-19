using MediatR;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.User.Commands.Auth.VerifyEmail
{
    public class VerifyEmailHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResponse>
    {
        readonly IUserRepository _users;
        readonly ICurrentUserRepository _current;
        public VerifyEmailHandler(IUserRepository users, ICurrentUserRepository current)
        {
            _users = users;
            _current = current;
        }

        public async Task<VerifyEmailResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var userId = _current.GetUserId();
            if (userId == null)
                return new VerifyEmailResponse { Success = false, StatusCode = 401, Message = "Unauthorized" };

            if (string.IsNullOrWhiteSpace(request.Token))
                return new VerifyEmailResponse { Success = false, StatusCode = 400, Message = "Token required" };

            var tokenEntity = await _users.GetEmailVerificationTokenAsync(request.Token);
            if (tokenEntity == null || tokenEntity.Used || tokenEntity.ExpiresUtc < DateTime.UtcNow || tokenEntity.UserId != userId.Value)
                return new VerifyEmailResponse { Success = false, StatusCode = 400, Message = "Invalid or expired token" };

            var user = await _users.GetByIdAsync(userId.Value);
            if (user == null)
                return new VerifyEmailResponse { Success = false, StatusCode = 404, Message = "User not found" };

            if (user.EmailVerified != null && user.EmailVerified == true)
                return new VerifyEmailResponse { Success = true, StatusCode = 200, Message = "Already verified", AlreadyVerified = true };

            await _users.MarkEmailVerifiedAsync(user);
            await _users.MarkEmailVerificationTokenUsedAsync(tokenEntity);

            return new VerifyEmailResponse { Success = true, StatusCode = 200, Message = "Email verified" };
        }
    }
}