using GiftApi.Application.Brand.DTOs;
using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Category.Queries.GetAll
{
    public class GetAllCategoriesResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetAllCategoriesItemsResponse> Items { get; set; } = new();
    }
    public class GetAllCategoriesItemsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Logo { get; set; }
        public List<BrandDto> Brands { get; set; } = new();
    }
}
