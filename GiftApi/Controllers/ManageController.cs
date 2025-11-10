using GiftApi.Application.Features.Manage.Brand.Commands.Create;
using GiftApi.Application.Features.Manage.Brand.Commands.Delete;
using GiftApi.Application.Features.Manage.Brand.Commands.Edit;
using GiftApi.Application.Features.Manage.Brand.Commands.Restore;
using GiftApi.Application.Features.Manage.Category.Commands.Create;
using GiftApi.Application.Features.Manage.Category.Commands.Delete;
using GiftApi.Application.Features.Manage.Category.Commands.Edit;
using GiftApi.Application.Features.Manage.Category.Commands.Restore;
using GiftApi.Application.Features.Manage.User.Commands.TopUpBalance;
using GiftApi.Application.Features.Manage.User.Queries.GetAllUsers;
using GiftApi.Application.Features.Manage.User.Queries.GetUser;
using GiftApi.Application.Features.Manage.Voucher.Commands.Create;
using GiftApi.Application.Features.Manage.Voucher.Commands.Edit;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.ChangeStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpPut("restore-category")]
        public async Task<RestoreCategoryResponse> RestoreCategory([FromQuery] RestoreCategoryCommand request) => await _mediator.Send(request);

        [HttpPost("create-brand")]
        public async Task<CreateBrandResponse> CreateBrand([FromBody] CreateBrandCommand request) => await _mediator.Send(request);

        [HttpPost("edit-brand")]
        public async Task<EditBrandResponse> EditBrand([FromBody] EditBrandCommand request) => await _mediator.Send(request);

        [HttpDelete("brand")]
        public async Task<DeleteBrandResponse> DeleteBrand([FromQuery] DeleteBrandCommand request) => await _mediator.Send(request);

        [HttpPut("restore-brand")]
        public async Task<RestoreBrandResponse> RestoreBrand([FromQuery] RestoreBrandCommand request) => await _mediator.Send(request);

        [HttpPost("create-voucher")]
        public async Task<CreateVoucherResponse> CreateVoucher([FromBody] CreateVoucherCommand request) => await _mediator.Send(request);
        
        [HttpPost("edit-voucher")]
        public async Task<EditVoucherResponse> EditVoucher([FromBody] EditVoucherCommand request) => await _mediator.Send(request);

        [HttpPut("to-pup-balance")]
        public async Task<TopUpBalanceResponse> TopUpBalance([FromQuery] TopUpBalanceCommand request) => await _mediator.Send(request);

        [HttpPut("change-delivery-info-status")]
        public async Task<ChangeStatusResponse> ChangeDeliveryInfoStatus([FromQuery] ChangeStatusCommand request) => await _mediator.Send(request);
    }
}
