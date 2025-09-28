using GiftApi.Application.Common.Responses;
using GiftApi.Common.Enums.User;

namespace GiftApi.Application.Manage.Queries.GetUsers
{
    public class GetUsersResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetUsersItemsResponse> Items { get; set; } = new();
    }
    public class GetUsersItemsResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string IdentificationNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public Gender? Gender { get; set; }
        public decimal Balance { get; set; }
        public UserType Type { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
