using MediatR;

namespace GiftApi.Application.Features.User.Queries.GetCurrent
{
    public class GetCurrentUserQuery : IRequest<GetCurrentUserResponse> { }
}