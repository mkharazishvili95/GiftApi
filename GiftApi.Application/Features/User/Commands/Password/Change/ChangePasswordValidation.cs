using FluentValidation;

namespace GiftApi.Application.Features.User.Commands.Password.Change
{
    public class ChangePasswordValidation : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordValidation()
        {
            RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Old password is required");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Confirm password is required");
            RuleFor(x => x.NewPassword)
                .MinimumLength(6).WithMessage("New password must be at least 6 characters");
            RuleFor(x => x.NewPassword)
                .Must((cmd, newPwd) => newPwd != cmd.OldPassword)
                .WithMessage("New password must be different from old password");
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }
}