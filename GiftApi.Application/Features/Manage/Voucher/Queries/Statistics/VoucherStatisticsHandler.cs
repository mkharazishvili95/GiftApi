using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public class VoucherStatisticsHandler : IRequestHandler<VoucherStatisticsQuery, VoucherStatisticsResponse>
    {
        readonly IVoucherStatisticsRepository _voucherStatisticsRepository;
        public VoucherStatisticsHandler(IVoucherStatisticsRepository voucherStatisticsRepository)
        {
            _voucherStatisticsRepository = voucherStatisticsRepository;
        }

        public Task<VoucherStatisticsResponse> Handle(VoucherStatisticsQuery request, CancellationToken cancellationToken) =>
            _voucherStatisticsRepository.GetVoucherStatisticsAsync(
                request.BrandId,
                request.FromUtc,
                request.ToUtc,
                request.LowStockThreshold,
                request.ExpiringInDays,
                request.IncludeInactive,
                request.TopSoldTake,
                cancellationToken);
    }
}