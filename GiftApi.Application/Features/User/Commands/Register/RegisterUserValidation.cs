using FluentValidation;
using GiftApi.Domain.Enums.User;
using System.Text.RegularExpressions;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.User.Commands.Register
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator(IUserRepository _userRepository)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Passwords do not match.");

            RuleFor(x => x.LoginType)
                .IsInEnum().WithMessage("Login type is invalid.");

            When(x => x.LoginType == LoginIdentifierType.Email, () =>
            {
                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Invalid email format.")
                    .MustAsync(async (email, ct) => !await _userRepository.EmailExists(email))
                    .WithMessage("Email already exists.");
            });

            When(x => x.LoginType == LoginIdentifierType.PhoneNumber, () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Must(IsValidPhoneNumber).WithMessage("Invalid phone number format.")
                    .MustAsync(async (phone, ct) => !await _userRepository.PhoneNumberExists(phone))
                    .WithMessage("Phone number already exists.");
            });

            RuleFor(x => x.IdentificationNumber)
                .NotEmpty().WithMessage("Identification number is required.")
                .Matches(@"^\d{11}$").WithMessage("Identification number must be 11 digits.")
                .MustAsync(async (id, ct) => !await _userRepository.IdentificationNumberExists(id))
                .WithMessage("Identification number already exists.");

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(BeAtLeast18YearsOld).WithMessage("User must be at least 18 years old.");
        }

        private bool IsValidPhoneNumber(string phoneNumber)
            => Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$");

        private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
            => dateOfBirth.AddYears(18) <= DateTime.UtcNow;
    }
}
