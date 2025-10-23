using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Edit
{
    public class EditCategoryCommand : IRequest<EditCategoryResponse>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
    }
}
