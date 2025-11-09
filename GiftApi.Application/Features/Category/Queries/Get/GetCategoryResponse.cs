using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Category.Queries.Get
{
    public class GetCategoryResponse : BaseResponse
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Logo { get; set; }
    }
}
