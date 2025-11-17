using GiftApi.Domain.Enums.File;
using MediatR;

namespace GiftApi.Application.Features.File.Commands.UpdateMeta
{
    public class UpdateFileMetaCommand : IRequest<UpdateFileMetaResponse>
    {
        public int FileId { get; set; }
        public string? FileName { get; set; }
        public FileType? FileType { get; set; }
        public bool? MainImage { get; set; }
    }
}