using GiftApi.Application.Features.Category.Queries.Get;
using GiftApi.Application.Features.Category.Queries.GetAll;
using GiftApi.Application.Features.Category.Queries.GetAllWithBrands;
using GiftApi.Application.Features.Category.Queries.GetWithBrands;
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

        [HttpPost("all")]
        public async Task<GetAllCategoriesResponse> GetAllCategories([FromBody] GetAllCategoriesQuery request) => await _mediator.Send(request);

        [HttpGet("with-brands")]
        public async Task<GetCategoryWithBrandsResponse> GetCategoryWithBrands([FromQuery] GetCategoryWithBrandsQuery request) => await _mediator.Send(request);

        [HttpPost("all-with-brands")]
        public async Task<GetAllCategoriesWithBrandsResponse> GetAllCategoriesWithBrands([FromBody] GetAllCategoriesWithBrandsQuery request) => await _mediator.Send(request);
    }
}
