using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;

namespace GiftApi.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly ApplicationDbContext _db;

        public FileRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<FileDto> UploadFileAsync(string? fileName, string? fileUrl, FileType? fileType)
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(fileUrl) || fileType == null)
                throw new ArgumentException("Invalid file data");

            var fileEntity = new GiftApi.Domain.Entities.File
            {
                FileName = fileName,
                FileUrl = fileUrl,
                FileType = fileType.Value,
                UploadDate = DateTime.UtcNow.AddHours(4),
                DeleteDate = null,
                IsDeleted = false
            };

            _db.Files.Add(fileEntity);
            await _db.SaveChangesAsync();

            return new FileDto
            {
                Id = fileEntity.Id,
                FileName = fileEntity.FileName,
                FileUrl = fileEntity.FileUrl,
                FileType = fileEntity.FileType
            };
        }
    }
}
