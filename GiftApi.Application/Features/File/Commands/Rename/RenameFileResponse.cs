using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.File.Commands.Rename
{
    public class RenameFileResponse : BaseResponse
    {
        public int FileId { get; set; }
        public string? NewFileName { get; set; }
        public string? FileUrl { get; set; }
    }
}
