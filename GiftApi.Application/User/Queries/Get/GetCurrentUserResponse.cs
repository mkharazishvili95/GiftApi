using GiftApi.Application.Common.Responses;
using GiftApi.Common.Enums.User;

namespace GiftApi.Application.User.Queries.Get
{
    public class GetCurrentUserResponse : BaseResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IdentificationNumber { get; set; }
        public string PhoneNumber { get; set; }
        public UserType Type { get; set; }
    }
}
