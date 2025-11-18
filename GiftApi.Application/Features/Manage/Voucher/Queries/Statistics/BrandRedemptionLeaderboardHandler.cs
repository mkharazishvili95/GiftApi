using MediatR;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public sealed class BrandRedemptionLeaderboardHandler : IRequestHandler<BrandRedemptionLeaderboardQuery, BrandRedemptionLeaderboardResponse>
    {
        readonly IVoucherStatisticsRepository _voucherStatisticsRepository;
        public BrandRedemptionLeaderboardHandler(IVoucherStatisticsRepository voucherStatisticsRepository)
        {
            _voucherStatisticsRepository = voucherStatisticsRepository;
        }

        public async Task<BrandRedemptionLeaderboardResponse> Handle(BrandRedemptionLeaderboardQuery request, CancellationToken ct)
        {
            var items = await _voucherStatisticsRepository.GetBrandRedemptionLeaderboardAsync(request.Top, ct);
            return new BrandRedemptionLeaderboardResponse
            {
                Items = items,
                TotalBrands = items.Count
            };
        }
    }
}