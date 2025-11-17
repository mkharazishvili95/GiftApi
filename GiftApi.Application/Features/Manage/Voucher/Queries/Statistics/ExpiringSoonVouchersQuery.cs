using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public class ExpiringSoonVouchersQuery : IRequest<List<VoucherStatisticsItemsResponse>>
    {
        public int? BrandId { get; set; }
        public int Days { get; set; } = 30;
        public bool IncludeInactive { get; set; } = false;
    }
}