using GiftApi.Application.Common.Models;
using GiftApi.Application.DTOs;

namespace GiftApi.Application.Features.User.Queries.GetMyPurchases
{
    public class GetMyPurchasesResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetMyPurchasesPurchasesItemsResponse> Items { get; set; } = new();
    }
    public class GetMyPurchasesPurchasesItemsResponse
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string? SenderName { get; set; }
        public string RecipientName { get; set; }
        public string? RecipientEmail { get; set; }
        public string RecipientPhone { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientAddress { get; set; }
        public string? Message { get; set; }
        public int? Quantity { get; set; }
        public bool? IsUsed { get; set; }
        public CategoryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
    }
}
