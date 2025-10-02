using GiftApi.Application.Category.DTOs;
using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Brand.Queries.GetAll
{
    public class GetAllBrandsResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetAllBrandsItemsResponse> Items { get; set; } = new();
    }
    public class GetAllBrandsItemsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public CategoryDto Category { get; set; } = new CategoryDto();
    }
}
