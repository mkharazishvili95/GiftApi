using GiftApi.Application.Voucher.Queries.Get;
using GiftApi.Application.Voucher.Queries.GetAll;
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

        [HttpGet("details")]
        public async Task<GetVoucherResponse> GetVoucher([FromQuery] GetVoucherQuery request) => await _mediator.Send(request);

        [HttpPost("all")]
        public async Task<GetAllVouchersResponse> GetAllVouchers([FromBody] GetAllVouchersQuery request) => await _mediator.Send(request);
    }
}
