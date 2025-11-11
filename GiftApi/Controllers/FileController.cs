using GiftApi.Application.Features.File.Commands.Delete;
using GiftApi.Application.Features.File.Commands.Rename;
using GiftApi.Application.Features.File.Commands.Upload;
using GiftApi.Application.Features.File.Queries.Get;
using GiftApi.Application.Features.File.Queries.GetAll;
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
