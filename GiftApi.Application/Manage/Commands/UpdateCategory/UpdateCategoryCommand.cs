using GiftApi.Application.Manage.Commands.UpdateCategory;
using MediatR;

namespace GiftApi.Application.Manage.Commands.EditCategory
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResponse>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
    }
}
