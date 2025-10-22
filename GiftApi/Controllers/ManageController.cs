using GiftApi.Application.Features.Manage.Queries.GetAllUsers;
using GiftApi.Application.Features.Manage.Queries.GetUser;
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

        [HttpPost("all-users")]
        public async Task<GetAllUsersResponse> GetAllUsersResponse([FromBody] GetAllUsersQuery request) => await _mediator.Send(request);
    }
}
