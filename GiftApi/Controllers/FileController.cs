using GiftApi.Application.Features.File.Commands.Delete;
using GiftApi.Application.Features.File.Commands.Rename;
using GiftApi.Application.Features.File.Commands.UpdateMeta;
using GiftApi.Application.Features.File.Commands.Upload;
using GiftApi.Application.Features.File.Queries.Get;
using GiftApi.Application.Features.File.Queries.GetAll;
using GiftApi.Application.Features.Manage.Brand.Commands.UploadLogo;
using GiftApi.Application.Features.Manage.Voucher.Commands.UploadImage;
using GiftApi.Domain.Enums.User;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        readonly IMediator _mediator;
        public FileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpPost("upload")]
        public async Task<UploadFileResponse> Upload([FromBody] UploadFileCommand request) => await _mediator.Send(request);

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpPost("upload-logo")]
        public async Task<ActionResult<UploadBrandLogoResponse>> UploadLogo([FromBody] UploadBrandLogoCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode ?? 200, result);
        }

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpPatch("voucher-image/{id:guid}")]
        public async Task<UploadVoucherImageResponse> ReplaceVoucherImage(Guid id, [FromBody] UploadVoucherImageCommand command)
        {
            command.Id = id;
            return await _mediator.Send(command);
        }

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpPatch("{id:int}/meta")]
        public async Task<IActionResult> PatchMeta(int id, [FromBody] UpdateFileMetaCommand command)
        {
            command.FileId = id;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode ?? 200, result);
        }

        [HttpGet]
        public async Task<GetFileResponse> Get([FromQuery] GetFileQuery request) => await _mediator.Send(request);

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpDelete]
        public async Task<DeleteFileResponse> Delete([FromQuery] DeleteFileCommand request) => await _mediator.Send(request);

        [Authorize(Roles = nameof(UserType.Admin))]
        [HttpPut("rename")]
        public async Task<RenameFileResponse> Rename([FromQuery] RenameFileCommand request) => await _mediator.Send(request);

        [HttpPost("all")]
        public async Task<GetAllFilesResponse> GetAll([FromBody] GetAllFilesQuery request) => await _mediator.Send(request);
    }
}
