using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetWithCategory
{
    public class GetBrandWithCategoryQuery : IRequest<GetBrandWithCategoryResponse>
    {
        public int Id { get; set; }
    }
}
