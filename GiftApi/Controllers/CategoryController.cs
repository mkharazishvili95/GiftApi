using GiftApi.Application.Features.Category.Queries.Get;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        readonly IMediator _mediator;
        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("details")]
        public async Task<GetCategoryResponse> GetCategory([FromQuery] GetCategoryQuery request) => await _mediator.Send(request);
    }
}
