using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.User.Commands.TopUpBalance
{
    public class TopUpBalanceResponse : BaseResponse
    {
        public decimal? NewBalance { get; set; }
        public decimal? OldBalance { get; set; }
        public string? UserName { get; set; }
    }
}
