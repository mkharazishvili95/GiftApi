using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Create
{
    public class CreateCategoryCommand : IRequest<CreateCategoryResponse>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Logo { get; set; }
    }
}
