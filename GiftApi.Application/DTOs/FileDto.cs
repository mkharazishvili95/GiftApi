using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.DTOs
{
    public class FileDto
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public FileType? FileType { get; set; }
    }
}
