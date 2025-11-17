using GiftApi.Application.Features.Manage.Brand.Commands.Create;
using GiftApi.Application.Features.Manage.Brand.Commands.Delete;
using GiftApi.Application.Features.Manage.Brand.Commands.Edit;
using GiftApi.Application.Features.Manage.Brand.Commands.Restore;
using GiftApi.Application.Features.Manage.Category.Commands.Create;
using GiftApi.Application.Features.Manage.Category.Commands.Delete;
using GiftApi.Application.Features.Manage.Category.Commands.Edit;
using GiftApi.Application.Features.Manage.Category.Commands.Restore;
using GiftApi.Application.Features.Manage.LoginAudit.Queries.GetAll;
using GiftApi.Application.Features.Manage.User.Commands.TopUpBalance;
using GiftApi.Application.Features.Manage.User.Queries.GetAllUsers;
using GiftApi.Application.Features.Manage.User.Queries.GetUser;
using GiftApi.Application.Features.Manage.Voucher.Commands.Activate;
using GiftApi.Application.Features.Manage.Voucher.Commands.BulkUpsert;
using GiftApi.Application.Features.Manage.Voucher.Commands.Create;
using GiftApi.Application.Features.Manage.Voucher.Commands.Delete;
using GiftApi.Application.Features.Manage.Voucher.Commands.Edit;
using GiftApi.Application.Features.Manage.Voucher.Commands.Restore;
using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.ChangeStatus;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.UndoRedeem;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Export;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Get;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.GetAll;
using GiftApi.Domain.Enums.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = nameof(UserType.Admin))]
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

        [HttpGet("purchase")]
        public async Task<GetPurchaseResponse> GetPurchase([FromQuery] GetPurchaseQuery request) => await _mediator.Send(request);

        [HttpPost("all-purchases")]
        public async Task<GetAllPurchasesResponse> GetAllPurchases([FromBody] GetAllPurchasesQuery request) => await _mediator.Send(request);

        [HttpPost("all-purchases/export")]
        public async Task<IActionResult> ExportAllPurchases([FromBody] ExportAllPurchasesQuery request)
        {
            var result = await _mediator.Send(request);
            return File(result.Data, result.ContentType, result.FileName);
        }

        [HttpPost("purchase/redeem")]
        public async Task<RedeemPurchaseResponse> RedeemPurchase([FromBody] RedeemPurchaseCommand request) => await _mediator.Send(request);

        [HttpPatch("purchases/{deliveryInfoId:guid}/undo-redeem")]
        public async Task<UndoRedeemPurchaseResponse> UndoRedeem(Guid deliveryInfoId, [FromBody] UndoRedeemPurchaseCommand command)
        {
            command.DeliveryInfoId = deliveryInfoId;
            return await _mediator.Send(command);
        }

        [HttpGet("login-audits")]
        public async Task<GetLoginAuditsResponse> GetLoginAudits([FromBody] GetLoginAuditsQuery request) => await _mediator.Send(request);

        [HttpPatch("{id:guid}/activate-voucher")]
        public async Task<IActionResult> ActivateVoucher(Guid id, [FromBody] ActivateVoucherCommand body)
        {
            if (body == null) return BadRequest("Body required.");
            body.Id = id;
            var result = await _mediator.Send(body);
            return StatusCode(result.StatusCode ?? 500, result);
        }

        [HttpDelete("voucher/{id:guid}")]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            var cmd = new DeleteVoucherCommand { Id = id };
            var result = await _mediator.Send(cmd);
            return StatusCode(result.StatusCode ?? 500, result);
        }

        [HttpPatch("voucher/{id:guid}/restore")]
        public async Task<IActionResult> RestoreVoucher(Guid id)
        {
            var cmd = new RestoreVoucherCommand { Id = id };
            var result = await _mediator.Send(cmd);
            return StatusCode(result.StatusCode ?? 500, result);
        }

        [HttpGet("voucher-statistics")]
        public async Task<VoucherStatisticsResponse> GetVoucherStatistics([FromQuery] VoucherStatisticsQuery query) => await _mediator.Send(query);

        [HttpPost("vouchers/bulk")]
        public async Task<IActionResult> BulkUpsertVouchers([FromBody] BulkUpsertVouchersCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode ?? 500, result);
        }
    }
}
