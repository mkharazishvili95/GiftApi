using GiftApi.Application.Manage.Queries.Get;
using GiftApi.Application.Manage.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManageController : Controller
    {
        readonly IMediator _mediator;
        public ManageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("users")]
        public async Task<GetUsersResponse> GetAllUsers(GetUsersQuery request) => await _mediator.Send(request);


        [HttpGet("user")]
        public async Task<GetUserResponse> GetUser([FromQuery]GetUserQuery request) => await _mediator.Send(request);
    }
}
