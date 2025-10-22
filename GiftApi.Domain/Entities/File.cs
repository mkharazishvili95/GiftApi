using GiftApi.Domain.Enums.File;

namespace GiftApi.Domain.Entities
{
    public class File
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
        public DateTime? UploadDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? UserId { get; set; }
        public bool? MainImage { get; set; }
    }
}
