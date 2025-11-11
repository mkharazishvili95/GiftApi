using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.GetAll
{
    public class GetAllPurchasesResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetAllPurchasesItemsResponse> Items { get; set; } = new();
    }
    public class GetAllPurchasesItemsResponse
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string? SenderName { get; set; }
        public string RecipientName { get; set; }
        public string? RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientAddress { get; set; }
        public string? SenderMessage { get; set; }
        public Guid? SenderId { get; set; }
        public int? Quantity { get; set; }
        public bool? IsUsed { get; set; }
    }
}
