using GiftApi.Application.Manage.Commands.CreateCategory;
using GiftApi.Application.Manage.Commands.DeleteCategory;
using GiftApi.Application.Manage.Commands.EditCategory;
using GiftApi.Application.Manage.Commands.RestoreCategory;
using GiftApi.Application.Manage.Commands.UpdateCategory;
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
        public async Task<GetUserResponse> GetUser([FromQuery] GetUserQuery request) => await _mediator.Send(request);

        [HttpPost("create-category")]
        public async Task<CreateCategoryResponse> CreateCategory([FromBody]CreateCategoryCommand request) => await _mediator.Send(request);

        [HttpPost("update-category")]
        public async Task<UpdateCategoryResponse> UpdateCategory([FromBody] UpdateCategoryCommand request) => await _mediator.Send(request);

        [HttpDelete("category")]
        public async Task<DeleteCategoryResponse> DeleteCategory([FromQuery] DeleteCategoryCommand request) => await _mediator.Send(request);

        [HttpPut("restore-category")]
        public async Task<RestoreCategoryResponse> RestoreCategory([FromQuery] RestoreCategoryCommand request) => await _mediator.Send(request);
    }
}
