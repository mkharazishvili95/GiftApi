using GiftApi.Application.Features.Manage.Category.Commands.Create;
using GiftApi.Application.Features.Manage.Category.Commands.Delete;
using GiftApi.Application.Features.Manage.Category.Commands.Edit;
using GiftApi.Application.Features.Manage.User.Queries.GetAllUsers;
using GiftApi.Application.Features.Manage.User.Queries.GetUser;
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

        [HttpGet("user")]
        public async Task<GetUserResponse> Get([FromQuery] GetUserQuery request) => await _mediator.Send(request);

        [HttpPost("all-users")]
        public async Task<GetAllUsersResponse> GetAllUsersResponse([FromBody] GetAllUsersQuery request) => await _mediator.Send(request);

        [HttpPost("create-category")]
        public async Task<CreateCategoryResponse> CreateCategory([FromBody] CreateCategoryCommand request) => await _mediator.Send(request);

        [HttpPut("edit-category")]
        public async Task<EditCategoryResponse> EditCategory([FromBody] EditCategoryCommand request) => await _mediator.Send(request);

        [HttpDelete("category")]
        public async Task<DeleteCategoryResponse> DeleteCategory([FromQuery] DeleteCategoryCommand request) => await _mediator.Send(request);
    }
}
