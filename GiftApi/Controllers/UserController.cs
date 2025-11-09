using GiftApi.Application.Features.User.Commands.Login;
using GiftApi.Application.Features.User.Commands.Login.RefreshToken;
using GiftApi.Application.Features.User.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        readonly IMediator _mediator;
        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<RegisterUserResponse> Register([FromBody] RegisterUserCommand request) => await _mediator.Send(request);

        [HttpPost("login")]
        public async Task<LoginUserResponse> Login([FromBody] LoginUserCommand request) => await _mediator.Send(request);

        [HttpPost("refresh-token")]
        public async Task<RefreshTokenResponse> RefreshToken([FromBody] RefreshTokenCommand command) => await _mediator.Send(command);
    }
}
