using GiftApi.Application.Common.Models;
using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.Features.File.Commands.Upload
{
    public class UploadFileResponse : BaseResponse
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
    }
}
