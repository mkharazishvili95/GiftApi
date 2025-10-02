using GiftApi.Common.Models;
using MediatR;

namespace GiftApi.Application.Category.Queries.GetAll
{
    public class GetAllCategoriesQuery : IRequest<GetAllCategoriesResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
