using MediatR;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.User.Commands.Auth.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutResponse>
    {
        readonly IUserRepository _userRepository;
        readonly ICurrentUserRepository _current;
        public LogoutHandler(IUserRepository userRepository, ICurrentUserRepository current)
        {
            _userRepository = userRepository;
            _current = current;
        }
        public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _current.GetUserId();
            if (userId == null)
                return new LogoutResponse { Success = false, StatusCode = 401, Message = "Unauthorized" };

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
                return new LogoutResponse { Success = false, StatusCode = 404, Message = "User not found" };

            await _userRepository.RevokeRefreshTokenAsync(user);
            return new LogoutResponse { Success = true, StatusCode = 200, Message = "Logged out" };
        }
    }
}