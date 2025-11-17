using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public class ExpiringSoonVouchersHandler : IRequestHandler<ExpiringSoonVouchersQuery, List<VoucherStatisticsItemsResponse>>
    {
        readonly IVoucherStatisticsRepository _voucherStatisticsRepository;
        public ExpiringSoonVouchersHandler(IVoucherStatisticsRepository voucherStatisticsRepository) => _voucherStatisticsRepository = voucherStatisticsRepository;

        public async Task<List<VoucherStatisticsItemsResponse>> Handle(ExpiringSoonVouchersQuery request, CancellationToken cancellationToken)
        {
            return await _voucherStatisticsRepository.GetExpiringSoonAsync(
                request.BrandId,
                request.Days,
                request.IncludeInactive,
                cancellationToken);
        }
    }
}