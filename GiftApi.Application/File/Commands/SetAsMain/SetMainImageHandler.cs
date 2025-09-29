using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.File.Commands.SetAsMain
{
    public class SetMainImageHandler : IRequestHandler<SetMainImageCommand, SetMainImageResponse>
    {
        readonly ApplicationDbContext _db;
        public SetMainImageHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<SetMainImageResponse> Handle(SetMainImageCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
