using GiftApi.Common.Models;
using MediatR;

namespace GiftApi.Application.Manage.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<GetUsersResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
