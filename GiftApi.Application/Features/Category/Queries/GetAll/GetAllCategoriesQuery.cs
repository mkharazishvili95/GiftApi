using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<GetAllCategoriesResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
