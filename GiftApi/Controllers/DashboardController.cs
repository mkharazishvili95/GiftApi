using GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics;
using GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary;
using GiftApi.Domain.Enums.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = nameof(UserType.Admin))]
    public class DashboardController : ControllerBase 
    {
        readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("summary")]
        public async Task<DashboardSummaryResponse> GetDashboardSummary([FromQuery] DashboardSummaryQuery query) => await _mediator.Send(query);

        [HttpGet]
        public async Task<GetGlobalMetricsResponse> Get([FromQuery] GetGlobalMetricsQuery query)  => await _mediator.Send(query);
    }
}
