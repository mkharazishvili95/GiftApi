using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Brand.Queries.Get
{
    public class GetBrandResponse : BaseResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? CategoryId { get; set; }
    }
}
