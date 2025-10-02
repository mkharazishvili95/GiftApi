using GiftApi.Application.File.Commands.Delete;
using GiftApi.Application.File.Commands.SetAsMain;
using GiftApi.Application.File.Commands.Upload;
using MediatR;
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

        [HttpPost("upload")]
        public async Task<FileUploadResponse> FileUpload(FileUploadCommand request) => await _mediator.Send(request);

        [HttpDelete("{id}")]
        public async Task<FileDeleteResponse> FileDelete([FromQuery] FileDeleteCommand request) => await _mediator.Send(request);

        [HttpPost("set-as-main")]
        public async Task<SetMainImageResponse> SetAsMainImage(SetMainImageCommand request) => await _mediator.Send(request);

    }
}
