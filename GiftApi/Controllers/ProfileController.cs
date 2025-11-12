using GiftApi.Application.Features.User.Commands.Password.Change;
using GiftApi.Application.Features.User.Queries.GetCurrent;
using GiftApi.Application.Features.User.Queries.GetMyPurchases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        readonly IMediator _mediator;
        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<GetCurrentUserResponse> Me() => await _mediator.Send(new GetCurrentUserQuery());

        [Authorize]
        [HttpGet("my-purchases")]
        public async Task<GetMyPurchasesResponse> MyPurchases([FromQuery] GetMyPurchasesQuery query) => await _mediator.Send(query);

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ChangePasswordResponse> ChangePassword([FromBody] ChangePasswordCommand command) => await _mediator.Send(command);
    }
}
