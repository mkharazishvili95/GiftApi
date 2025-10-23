using GiftApi.Application.Features.File.Commands.Delete;
using GiftApi.Application.Features.File.Commands.Upload;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GiftApi.Controllers
{
    [ApiController]
    [Route("api/file")]
    public class FileController : ControllerBase
    {
        readonly IMediator _mediator;
        public FileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("upload")]
        public async Task<UploadFileResponse> Upload([FromBody] UploadFileCommand request) => await _mediator.Send(request);

        [HttpDelete]
        public async Task<DeleteFileResponse> Delete([FromQuery] DeleteFileCommand request) => await _mediator.Send(request);
    }
}
