using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.BulkUpsert
{
    public class BulkUpsertVouchersCommand : IRequest<BulkUpsertVouchersResponse>
    {
        public List<BulkUpsertVouchersItemsResponse> Items { get; set; } = new();
    }

    public class BulkUpsertVouchersItemsResponse
    {
        public Guid? Id { get; set; }   
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsPercentage { get; set; }
        public int? BrandId { get; set; }
        public int ValidMonths { get; set; } = 6;
        public bool Unlimited { get; set; }
        public int Quantity { get; set; }
        public int Redeemed { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }
}