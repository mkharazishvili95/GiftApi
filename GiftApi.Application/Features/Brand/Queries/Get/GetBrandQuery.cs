using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.Get
{
    public class GetBrandQuery : IRequest<GetBrandResponse>
    {
        public int Id { get; set; }
    }
}
