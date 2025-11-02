using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Delete
{
    public class DeleteBrandCommand : IRequest<DeleteBrandResponse>
    {
        public int Id { get; set; }
    }
}
