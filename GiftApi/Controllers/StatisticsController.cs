using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        readonly IMediator _mediator;
        public StatisticsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("voucher-usage")]
        public async Task<ActionResult<VoucherUsageStatsResponse>> GetVoucherUsage(
            [FromQuery] int? brandId,
            [FromQuery] string? search,
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? orderBy = null,
            [FromQuery] bool desc = true,
            CancellationToken ct = default)
        {
            var query = new VoucherUsageStatsQuery
            {
                BrandId = brandId,
                Search = search,
                IncludeInactive = includeInactive,
                OrderBy = orderBy,
                Desc = desc
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("vouchers-expiring-soon")]
        public async Task<ActionResult<List<VoucherStatisticsItemsResponse>>> GetExpiringSoon( [FromQuery] int? brandId, [FromQuery] int days = 30, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        {
            var query = new ExpiringSoonVouchersQuery
            {
                BrandId = brandId,
                Days = days,
                IncludeInactive = includeInactive
            };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpGet("usage-trend")]
        public async Task<ActionResult<VoucherUsageTrendResponse>> GetUsageTrend( [FromQuery] int? brandId, [FromQuery] int days = 30, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new VoucherUsageTrendQuery(brandId, days), ct);
            return Ok(result);
        }

        [HttpGet("brand-redemption-leaderboard")]
        public async Task<ActionResult<BrandRedemptionLeaderboardResponse>> GetBrandLeaderboard(  [FromQuery] int top = 10, CancellationToken ct = default)
        {
            var result = await _mediator.Send(new BrandRedemptionLeaderboardQuery(top), ct);
            return Ok(result);
        }
    }
}
