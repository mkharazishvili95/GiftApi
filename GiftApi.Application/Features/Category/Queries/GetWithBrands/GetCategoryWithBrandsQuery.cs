using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetWithBrands
{
    public class GetCategoryWithBrandsQuery : IRequest<GetCategoryWithBrandsResponse>
    {
        public int CategoryId { get; set; }
    }
}
