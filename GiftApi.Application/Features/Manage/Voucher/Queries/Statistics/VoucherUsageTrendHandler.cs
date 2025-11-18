using MediatR;
using GiftApi.Application.Interfaces;

namespace GiftApi.Application.Features.Manage.Voucher.Queries.Statistics
{
    public sealed class VoucherUsageTrendHandler : IRequestHandler<VoucherUsageTrendQuery, VoucherUsageTrendResponse>
    {
        readonly IVoucherStatisticsRepository _voucherStatisticsRepository;
        public VoucherUsageTrendHandler(IVoucherStatisticsRepository voucherStatisticsRepository)
        {
            _voucherStatisticsRepository = voucherStatisticsRepository;
        }

        public async Task<VoucherUsageTrendResponse> Handle(VoucherUsageTrendQuery request, CancellationToken ct)
        {
            var rows = await _voucherStatisticsRepository.GetDailyUsageAsync(request.BrandId, request.Days, ct);
            var response = new VoucherUsageTrendResponse
            {
                Points = rows
                    .OrderBy(r => r.Day)
                    .Select(r => new VoucherUsageTrendPoint
                    {
                        Day = DateOnly.FromDateTime(r.Day),
                        Sold = r.Sold,
                        Redeemed = r.Redeemed
                    }).ToList()
            };
            response.TotalSold = response.Points.Sum(p => p.Sold);
            response.TotalRedeemed = response.Points.Sum(p => p.Redeemed);
            return response;
        }
    }
}