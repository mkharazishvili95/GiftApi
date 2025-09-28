using MediatR;

namespace GiftApi.Application.Manage.Commands.RestoreCategory
{
    public class RestoreCategoryCommand : IRequest<RestoreCategoryResponse>
    {
        public int Id { get; set; }
    }
}
