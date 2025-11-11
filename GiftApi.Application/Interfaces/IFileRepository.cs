using GiftApi.Application.DTOs;
using GiftApi.Domain.Entities;
using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.Interfaces
{
    public interface IFileRepository
    {
        Task<FileDto> UploadFileAsync(string? fileName, string? fileUrl, FileType? fileType);
        Task<GiftApi.Domain.Entities.File?> GetFileAsync(int id);
        Task<bool> DeleteFileAsync(int fileId);
        Task EditFile(Domain.Entities.File? file);
        Task<List<GiftApi.Domain.Entities.File>?> GetAll();
    }
}