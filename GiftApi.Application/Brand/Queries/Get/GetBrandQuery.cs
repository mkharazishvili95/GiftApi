using MediatR;

namespace GiftApi.Application.Brand.Queries.Get
{
    public class GetBrandQuery : IRequest<GetBrandResponse>
    {
        public int Id { get; set; }
    }
}
