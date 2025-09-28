using GiftApi.Application.Brand.Queries.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        readonly IMediator _mediator;
        public BrandController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<GetBrandResponse> GetBrand([FromQuery] GetBrandQuery request) => await _mediator.Send(request);
    }
}
