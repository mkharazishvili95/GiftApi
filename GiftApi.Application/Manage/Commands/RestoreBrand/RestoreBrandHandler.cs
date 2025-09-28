using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.RestoreBrand
{
    public class RestoreBrandHandler : IRequestHandler<RestoreBrandCommand, RestoreBrandResponse>
    {
        readonly ApplicationDbContext _db;
        public RestoreBrandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RestoreBrandResponse> Handle(RestoreBrandCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new RestoreBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand Id is required." };

            var brand = await _db.Brands.FindAsync(request.Id);
            if (brand == null)
                return new RestoreBrandResponse { StatusCode = 404, Success = false, UserMessage = "Brand not found." };

            if (!brand.IsDeleted)
                return new RestoreBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand is not deleted." };

            brand.IsDeleted = false;
            brand.DeleteDate = null;
            brand.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(brand);
            await _db.SaveChangesAsync(cancellationToken);

            return new RestoreBrandResponse { StatusCode = 200, Success = true, UserMessage = "Brand restored successfully." };
        }
    }
}
