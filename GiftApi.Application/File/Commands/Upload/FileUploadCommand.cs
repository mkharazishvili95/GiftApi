using GiftApi.Common.Enums.File;
using MediatR;

namespace GiftApi.Application.File.Commands.Upload
{
    public class FileUploadCommand : IRequest<FileUploadResponse>
    {
        public string? FileName { get; set; }
        public string? FileContent { get; set; }
        public FileType? FileType { get; set; }
    }
}
