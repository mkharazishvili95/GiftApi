using GiftApi.Application.Features.User.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<RegisterUserResponse> Register([FromBody] RegisterUserCommand request) => await _mediator.Send(request);
    }
}
