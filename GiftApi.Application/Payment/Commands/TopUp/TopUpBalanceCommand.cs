using MediatR;

namespace GiftApi.Application.Payment.Commands.TopUp
{
    public class TopUpBalanceCommand : IRequest<TopUpBalanceResponse>
    {
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
    }
}
