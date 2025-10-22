using MediatR;

namespace GiftApi.Application.Features.Manage.Queries.Get
{
    public class GetUserQuery : IRequest<GetUserResponse>
    {
        public Guid Id { get; set; }
    }
}
