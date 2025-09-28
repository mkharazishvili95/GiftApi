using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.User.Commands.Login
{
    public class LoginUserResponse : BaseResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
