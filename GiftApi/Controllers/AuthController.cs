using GiftApi.Application.Features.User.Commands.Register;
using GiftApi.Application.Features.User.Commands.Login;
using GiftApi.Application.Features.User.Commands.Login.RefreshToken;
using GiftApi.Application.Features.User.Commands.Auth.Logout;
using GiftApi.Application.Features.User.Commands.Auth.ForgotPassword;
using GiftApi.Application.Features.User.Commands.Auth.ResetPassword;
using GiftApi.Application.Features.User.Commands.Auth.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public Task<RegisterUserResponse> Register([FromBody] RegisterUserCommand cmd) => _mediator.Send(cmd);

        [AllowAnonymous]
        [HttpPost("login")]
        public Task<LoginUserResponse> Login([FromBody] LoginUserCommand cmd) => _mediator.Send(cmd);

        [AllowAnonymous]
        [HttpPost("refresh")]
        public Task<RefreshTokenResponse> Refresh([FromBody] RefreshTokenCommand cmd) => _mediator.Send(cmd);

        [Authorize]
        [HttpPost("logout")]
        public Task<LogoutResponse> Logout([FromBody] LogoutCommand cmd) => _mediator.Send(cmd);

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public Task<ForgotPasswordResponse> Forgot([FromBody] ForgotPasswordCommand cmd) => _mediator.Send(cmd);

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public Task<ResetPasswordResponse> Reset([FromBody] ResetPasswordCommand cmd) => _mediator.Send(cmd);

        [Authorize]
        [HttpPost("verify-email")]
        public Task<VerifyEmailResponse> Verify([FromBody] VerifyEmailCommand cmd) => _mediator.Send(cmd);
    }
}