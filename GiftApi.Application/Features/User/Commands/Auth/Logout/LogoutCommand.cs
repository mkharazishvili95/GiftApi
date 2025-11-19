using MediatR;

namespace GiftApi.Application.Features.User.Commands.Auth.Logout
{
    public class LogoutCommand : IRequest<LogoutResponse>
    {
        public bool RevokeAll { get; set; }
    }
}