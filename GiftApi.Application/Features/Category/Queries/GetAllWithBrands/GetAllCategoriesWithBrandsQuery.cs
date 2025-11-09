using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetAllWithBrands
{
    public class GetAllCategoriesWithBrandsQuery : IRequest<GetAllCategoriesWithBrandsResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
