using GiftApi.Domain.Enums.User;
using MediatR;

namespace GiftApi.Application.Features.User.Commands.Register
{
    public class RegisterUserCommand : IRequest<RegisterUserResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public Gender? Gender { get; set; }
        public LoginIdentifierType LoginType { get; set; } = LoginIdentifierType.PhoneNumber;
    }
}
