using FluentValidation;
using GiftApi.Application.User.Commands.Register;
using GiftApi.Common.Enums.User;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GiftApi.Application.User.Validators
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        readonly ApplicationDbContext _db;

        public RegisterUserValidator(ApplicationDbContext db)
        {
            _db = db;

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
                    .MustAsync(async (email, ct) => !await _db.Users.AnyAsync(u => u.Email == email, ct))
                    .WithMessage("Email already exists.");
            });

            When(x => x.LoginType == LoginIdentifierType.PhoneNumber, () =>
            {
                RuleFor(x => x.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .Must(IsValidPhoneNumber).WithMessage("Invalid phone number format.")
                    .MustAsync(async (phone, ct) => !await _db.Users.AnyAsync(u => u.PhoneNumber == phone, ct))
                    .WithMessage("Phone number already exists.");
            });

            RuleFor(x => x.DateOfBirth)
                .NotEmpty().WithMessage("Date of birth is required.")
                .Must(BeAtLeast18YearsOld).WithMessage("User must be at least 18 years old.");
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            var phoneRegex = new Regex(@"^\+?\d{7,15}$");
            return phoneRegex.IsMatch(phoneNumber);
        }

        private bool BeAtLeast18YearsOld(DateTime dateOfBirth)
        {
            return dateOfBirth.AddYears(18) <= DateTime.UtcNow;
        }
    }
}
