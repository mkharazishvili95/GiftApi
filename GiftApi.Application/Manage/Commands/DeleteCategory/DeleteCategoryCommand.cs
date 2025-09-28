using MediatR;

namespace GiftApi.Application.Manage.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResponse>
    {
        public int Id { get; set; }
    }
}
