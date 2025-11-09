using MediatR;

namespace GiftApi.Application.Features.Manage.User.Commands.TopUpBalance
{
    public class TopUpBalanceCommand : IRequest<TopUpBalanceResponse>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
