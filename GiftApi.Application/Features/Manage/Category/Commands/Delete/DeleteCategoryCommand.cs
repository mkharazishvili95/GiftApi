using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Delete
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResponse>
    {
        public int Id { get; set; }
    }
}
