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
            [FromQuery] bool desc = true)
        {
            var query = new VoucherUsageStatsQuery
            {
                BrandId = brandId,
                Search = search,
                IncludeInactive = includeInactive,
                OrderBy = orderBy,
                Desc = desc
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
