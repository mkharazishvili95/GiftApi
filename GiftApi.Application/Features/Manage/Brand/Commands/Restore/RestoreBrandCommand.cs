using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Restore
{
    public class RestoreBrandCommand : IRequest<RestoreBrandResponse>
    {
        public int Id { get; set; }
    }
}
