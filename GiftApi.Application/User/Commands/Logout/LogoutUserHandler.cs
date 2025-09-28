using GiftApi.Application.Common.Responses;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.User.Commands.Logout
{
    public class LogoutUserHandler : IRequestHandler<LogoutUserCommand, BaseResponse>
    {
        private readonly ApplicationDbContext _db;

        public LogoutUserHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<BaseResponse> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

            if (user == null)
            {
                return new BaseResponse
                {
                    Success = false,
                    UserMessage = "Invalid refresh token.",
                    StatusCode = 400
                };
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            _db.Users.Update(user);
            await _db.SaveChangesAsync(cancellationToken);

            return new BaseResponse
            {
                Success = true,
                UserMessage = "Logout successful",
                StatusCode = 200
            };
        }
    }
}
