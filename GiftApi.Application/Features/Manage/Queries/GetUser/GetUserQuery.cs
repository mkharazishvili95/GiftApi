using MediatR;

namespace GiftApi.Application.Features.Manage.Queries.GetUser
{
    public class GetUserQuery : IRequest<GetUserResponse>
    {
        public Guid Id { get; set; }
    }
}
