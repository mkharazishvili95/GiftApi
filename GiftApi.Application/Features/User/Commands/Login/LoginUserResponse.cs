using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.User.Commands.Login
{
    public class LoginUserResponse : BaseResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
