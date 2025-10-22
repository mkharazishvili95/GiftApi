using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.User.Commands.Register
{
    public class RegisterUserResponse : BaseResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
    }
}
