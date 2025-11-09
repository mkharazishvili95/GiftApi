using GiftApi.Application.Features.Brand.Queries.Get;
using GiftApi.Application.Features.Brand.Queries.GetAll;
using GiftApi.Application.Features.Brand.Queries.GetWithCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        readonly IMediator _mediator;
        public BrandController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("details")]
        public async Task<GetBrandResponse> GetBrand([FromQuery] GetBrandQuery request) => await _mediator.Send(request);

        [HttpPost("all")]
        public async Task<GetAllBrandsResponse> GetAllBrands([FromBody] GetAllBrandsQuery request) => await _mediator.Send(request);

        [HttpGet("with-category")]
        public async Task<GetBrandWithCategoryResponse> GetBrandWithCategory([FromQuery] GetBrandWithCategoryQuery request) => await _mediator.Send(request);
    }
}
