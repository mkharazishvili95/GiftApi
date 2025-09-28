using MediatR;

namespace GiftApi.Application.Manage.Commands.DeleteBrand
{
    public class DeleteBrandCommand : IRequest<DeleteBrandResponse>
    {
        public int Id { get; set; }
    }
}
