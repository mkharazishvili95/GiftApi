using MediatR;

namespace GiftApi.Application.User.Queries.Get
{
    public class GetCurrentUserQuery : IRequest<GetCurrentUserResponse> { }
}
