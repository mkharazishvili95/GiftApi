using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.File.Commands.Delete
{
    public class FileDeleteHandler : IRequestHandler<FileDeleteCommand, FileDeleteResponse>
    {
        readonly ApplicationDbContext _db;
        public FileDeleteHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<FileDeleteResponse> Handle(FileDeleteCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
