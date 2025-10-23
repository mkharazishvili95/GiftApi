using MediatR;

namespace GiftApi.Application.Features.Manage.User.Queries.GetUser
{
    public class GetUserQuery : IRequest<GetUserResponse>
    {
        public Guid Id { get; set; }
    }
}
