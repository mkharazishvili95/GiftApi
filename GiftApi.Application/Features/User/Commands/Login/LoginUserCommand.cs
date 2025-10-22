using MediatR;

namespace GiftApi.Application.Features.User.Commands.Login
{
    public class LoginUserCommand : IRequest<LoginUserResponse>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
