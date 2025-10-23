using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Category.Commands.Create
{
    public class CreateCategoryResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
    }
}
