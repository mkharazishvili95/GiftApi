using GiftApi.Application.DTOs;
using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.Interfaces
{
    public interface IFileRepository
    {
        Task<FileDto> UploadFileAsync(string? fileName, string? fileUrl, FileType? fileType);
    }
}