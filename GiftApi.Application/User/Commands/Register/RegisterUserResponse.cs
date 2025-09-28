using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.User.Commands.Register
{
    public class RegisterUserResponse : BaseResponse
    {
        public string UserName { get; set; }
    }
}
