using GiftApi.Application.Brand.DTOs;
using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Category.Queries.Get
{
    public class GetCategoryResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Logo { get; set; }
        public List<BrandDto> Brands { get; set; } = new ();
    }
}
