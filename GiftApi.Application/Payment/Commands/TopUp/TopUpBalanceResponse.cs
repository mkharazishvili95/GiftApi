using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Payment.Commands.TopUp
{
    public class TopUpBalanceResponse : BaseResponse
    {
        public decimal NewBalance { get; set; }
    }
}
