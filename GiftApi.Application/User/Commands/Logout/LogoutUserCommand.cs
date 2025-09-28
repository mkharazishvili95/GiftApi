using GiftApi.Application.Common.Responses;
using MediatR;

namespace GiftApi.Application.User.Commands.Logout
{
    public class LogoutUserCommand : IRequest<BaseResponse>
    {
        public string RefreshToken { get; set; }
    }
}
