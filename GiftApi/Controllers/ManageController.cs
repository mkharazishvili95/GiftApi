using GiftApi.Application.Features.Manage.Queries.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/manage")]
    public class ManageController : ControllerBase
    {
        readonly IMediator _mediator;
        public ManageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<GetUserResponse> Get([FromQuery] GetUserQuery request) => await _mediator.Send(request);
    }
}
