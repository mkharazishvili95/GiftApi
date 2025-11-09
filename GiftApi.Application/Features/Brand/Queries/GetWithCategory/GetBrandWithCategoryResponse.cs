using GiftApi.Application.Common.Models;
using GiftApi.Application.DTOs;

namespace GiftApi.Application.Features.Brand.Queries.GetWithCategory
{
    public class GetBrandWithCategoryResponse : BaseResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? CategoryId { get; set; }
        public CategoryDto? Category { get; set; }
    }
}
