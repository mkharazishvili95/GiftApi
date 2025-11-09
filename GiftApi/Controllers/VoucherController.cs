using GiftApi.Application.Features.Voucher.Queries.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        readonly IMediator _mediator;
        public VoucherController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<GetVoucherResponse> Get([FromQuery] GetVoucherQuery query) => await _mediator.Send(query);
    }
}
