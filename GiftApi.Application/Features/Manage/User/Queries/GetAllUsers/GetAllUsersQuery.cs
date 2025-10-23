using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Manage.User.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<GetAllUsersResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
