using GiftApi.Domain.Enums.File;
using MediatR;

namespace GiftApi.Application.Features.File.Commands.Upload
{
    public class UploadFileCommand : IRequest<UploadFileResponse>
    {
        public string? FileName { get; set; }
        public string? FileContent { get; set; }
        public FileType? FileType { get; set; }
    }
}
