using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Create
{
    public class CreateVoucherCommand : IRequest<CreateVoucherResponse>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; } = false;
        public int? BrandId { get; set; }
        public int ValidMonths { get; set; } = 6;
        public bool Unlimited { get; set; }
        public int Quantity { get; set; }
        public int Redeemed { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}
