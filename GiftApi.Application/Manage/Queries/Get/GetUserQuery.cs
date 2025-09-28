using MediatR;

namespace GiftApi.Application.Manage.Queries.Get
{
    public class GetUserQuery : IRequest<GetUserResponse>
    {
        public Guid Id { get; set; }
    }
}
