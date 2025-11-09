using GiftApi.Application.Common.Models;
using GiftApi.Application.DTOs;

namespace GiftApi.Application.Features.Category.Queries.GetWithBrands
{
    public class GetCategoryWithBrandsResponse : BaseResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Logo { get; set; }
        public List<BrandDto> Brands { get; set; } = new List<BrandDto>();
    }
}
