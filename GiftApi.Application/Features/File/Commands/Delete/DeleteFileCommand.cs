using MediatR;

namespace GiftApi.Application.Features.File.Commands.Delete
{
    public class DeleteFileCommand : IRequest<DeleteFileResponse>
    {
        public int FileId { get; set; }
    }
}
