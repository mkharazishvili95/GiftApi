using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetAllWithCategories
{
    public class GetAllBrandsWithCategoriesQuery : IRequest<GetAllBrandsWithCategoriesResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
