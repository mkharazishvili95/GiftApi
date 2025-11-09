using FluentValidation;

namespace GiftApi.Application.Features.Voucher.Commands.Buy
{
    public class BuyVoucherValidation : AbstractValidator<BuyVoucherCommand>
    {
        public BuyVoucherValidation()
        {
            RuleFor(x => x.VoucherId)
                .NotEmpty().WithMessage("VoucherId is required.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required.")
                .MaximumLength(100).WithMessage("Recipient name must not exceed 100 characters.");

            RuleFor(x => x.RecipientCity)
                .NotEmpty().WithMessage("Recipient city is required.")
                .MaximumLength(100).WithMessage("Recipient city must not exceed 100 characters.");

            RuleFor(x => x.RecipientAddress)
                .NotEmpty().WithMessage("Recipient address is required.")
                .MaximumLength(200).WithMessage("Recipient address must not exceed 200 characters.");

            RuleFor(x => x.RecipientPhone)
                .NotEmpty().WithMessage("Recipient phone is required.")
                .Matches(@"^5\d{8}$").WithMessage("Recipient phone must be a valid Georgian mobile number starting with '5' (e.g., 598123456).");
        }
    }
}