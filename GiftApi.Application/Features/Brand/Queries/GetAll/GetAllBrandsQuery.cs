using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetAll
{
    public class GetAllBrandsQuery : IRequest<GetAllBrandsResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
