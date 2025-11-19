using MediatR;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.User.Commands.Auth.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
    {
        readonly IUserRepository _users;
        public ForgotPasswordHandler(IUserRepository users)
        {
            _users = users;
        }

        public async Task<ForgotPasswordResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return new ForgotPasswordResponse { Success = false, StatusCode = 400, Message = "Email required" };

            var user = await _users.GetByEmailAsync(request.Email);
            if (user == null)
                return new ForgotPasswordResponse { Success = true, StatusCode = 200, Message = "If account exists, email sent" };

            string rawToken = Guid.NewGuid().ToString("N");
            await _users.CreatePasswordResetTokenAsync(user.Id, rawToken, TimeSpan.FromHours(1));

            return new ForgotPasswordResponse { Success = true, StatusCode = 200, Message = "Reset instructions sent", PublicToken = rawToken };
        }
    }
}