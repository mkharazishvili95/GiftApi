using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats
{
    public class VoucherUsageStatsHandler : IRequestHandler<VoucherUsageStatsQuery, VoucherUsageStatsResponse>
    {
        private readonly IVoucherStatisticsRepository _repo;
        public VoucherUsageStatsHandler(IVoucherStatisticsRepository repo) => _repo = repo;

        public Task<VoucherUsageStatsResponse> Handle(VoucherUsageStatsQuery request, CancellationToken cancellationToken) =>
            _repo.GetVoucherUsageStatsAsync(
                request.BrandId,
                request.Search,
                request.IncludeInactive,
                request.OrderBy,
                request.Desc,
                cancellationToken);
    }
}