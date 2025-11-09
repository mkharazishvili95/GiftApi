using GiftApi.Application.Features.Voucher.Commands.Buy;
using GiftApi.Application.Features.Voucher.Queries.Get;
using GiftApi.Application.Features.Voucher.Queries.GetAll;
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

        [HttpPost("all")]
        public async Task<GetAllVouchersResponse> GetAll([FromBody] GetAllVouchersQuery query) => await _mediator.Send(query);

        [HttpPost("buy")]
        public async Task<BuyVoucherResponse> Buy([FromBody] BuyVoucherCommand command) => await _mediator.Send(command);
    }
}
