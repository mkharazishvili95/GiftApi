using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Manage.Commands.CreateCategory
{
    public class CreateCategoryResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
    }
}
