using FluentValidation;
using GiftApi.Application.User.Commands.Login;

namespace GiftApi.Application.User.Validators
{
    public class LoginUserValidator : AbstractValidator<LoginUserCommand>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
