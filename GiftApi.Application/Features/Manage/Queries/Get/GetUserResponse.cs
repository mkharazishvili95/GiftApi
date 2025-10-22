using GiftApi.Application.Common.Models;
using GiftApi.Domain.Enums.User;

namespace GiftApi.Application.Features.Manage.Queries.Get
{
    public class GetUserResponse : BaseResponse
    {
        public Guid? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? IdentificationNumber { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public Gender? Gender { get; set; }
        public decimal? Balance { get; set; }
        public UserType? Type { get; set; }
        public DateTime? RegisterDate { get; set; }
    }
}
