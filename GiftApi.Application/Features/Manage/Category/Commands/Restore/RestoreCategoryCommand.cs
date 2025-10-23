using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Restore
{
    public class RestoreCategoryCommand : IRequest<RestoreCategoryResponse>
    {
        public int Id { get; set; }
    }
}
