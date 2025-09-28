using MediatR;

namespace GiftApi.Application.Manage.Commands.RestoreBrand
{
    public class RestoreBrandCommand : IRequest<RestoreBrandResponse>
    {
        public int Id { get; set; }
    }
}
