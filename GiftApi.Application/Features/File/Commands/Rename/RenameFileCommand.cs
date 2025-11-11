using MediatR;

namespace GiftApi.Application.Features.File.Commands.Rename
{
    public class RenameFileCommand : IRequest<RenameFileResponse>
    {
        public int FileId { get; set; }
        public string? NewFileName { get; set; }
    }
}
