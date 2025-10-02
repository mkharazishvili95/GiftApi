using GiftApi.Application.Payment.Commands.TopUp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        readonly IMediator _mediator;
        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("to-pup-balance")]
        public async Task<TopUpBalanceResponse> TopUpBalance([FromQuery] TopUpBalanceCommand request) => await _mediator.Send(request);
    }
}
