using MediatR;

namespace GiftApi.Application.Features.Voucher.Commands.Buy
{
    public class BuyVoucherCommand : IRequest<BuyVoucherResponse>
    {
        public Guid VoucherId { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        public string RecipientName { get; set; } = default!;
        public string RecipientPhone { get; set; } = default!;
        public string RecipientCity { get; set; } = default!;
        public string RecipientAddress { get; set; } = default!;
        public string? RecipientEmail { get; set; }
        public string? Message { get; set; }
        public string? SenderName { get; set; }

    }
}
