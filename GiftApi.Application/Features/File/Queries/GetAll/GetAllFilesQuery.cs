using GiftApi.Application.Common.Models;
using GiftApi.Domain.Enums.File;
using MediatR;

namespace GiftApi.Application.Features.File.Queries.GetAll
{
    public class GetAllFilesQuery : IRequest<GetAllFilesResponse>
    {
        public PaginationModel Pagination { get; set; } = new();
        public string? FileName { get; set; }
        public FileType? FileType { get; set; }
        public Guid? UserId { get; set; }
        public bool? MainImage { get; set; }
        public DateTime? UploadDateFrom { get; set; }
        public DateTime? UploadDateTo { get; set; }
    }
}
