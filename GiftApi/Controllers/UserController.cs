using GiftApi.Application.User.Commands.Login;
using GiftApi.Application.User.Commands.Logout;
using GiftApi.Application.User.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);

            if ((bool)!result.Success)
            {
                return StatusCode((int)result.StatusCode, new
                {
                    message = result.UserMessage
                });
            }

            return Ok(new
            {
                message = result.UserMessage,
                userName = result.UserName
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        {
            var result = await _mediator.Send(command);

            if ((bool)!result.Success)
            {
                return StatusCode((int)result.StatusCode, new
                {
                    message = result.UserMessage
                });
            }

            return Ok(new
            {
                message = result.UserMessage,
                accessToken = result.AccessToken,
                refreshToken = result.RefreshToken
            });
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutUserCommand command)
        {
            var result = await _mediator.Send(command);

            if ((bool)!result.Success)
                return StatusCode((int)result.StatusCode, new { message = result.UserMessage });

            return Ok(new { message = result.UserMessage });
        }
    }
}
