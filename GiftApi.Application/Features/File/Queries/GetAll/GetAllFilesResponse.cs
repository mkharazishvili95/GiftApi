using GiftApi.Application.Common.Models;
using GiftApi.Domain.Enums.File;

namespace GiftApi.Application.Features.File.Queries.GetAll
{
    public class GetAllFilesResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<GetAllFilesItemsResponse> Items { get; set; } = new();
    }
    public class GetAllFilesItemsResponse
    {
        public int? Id { get; set; }
        public string? FileName { get; set; }
        public FileType? FileType { get; set; }
        public Guid? UserId { get; set; }
        public bool? MainImage { get; set; }
        public DateTime? UploadDate { get; set; }
    }
}
