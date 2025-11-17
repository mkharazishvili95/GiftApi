using GiftApi.Application.Common.Models;
using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.Features.File.Commands.UpdateMeta
{
    public class UpdateFileMetaResponse : BaseResponse
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public DateTime? UploadDate { get; set; }
        public Guid? UserId { get; set; }
        public bool? MainImage { get; set; }
    }
}