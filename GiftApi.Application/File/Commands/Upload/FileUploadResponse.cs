using GiftApi.Application.Common.Responses;
using GiftApi.Common.Enums.File;

namespace GiftApi.Application.File.Commands.Upload
{
    public class FileUploadResponse : BaseResponse
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
    }
}
