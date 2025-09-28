using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.DeleteBrand
{
    public class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand, DeleteBrandResponse>
    {
        readonly ApplicationDbContext _db;
        public DeleteBrandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteBrandResponse> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new DeleteBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand Id is required." };

            var brand = await _db.Brands.FindAsync(request.Id);

            if (brand == null || brand.IsDeleted)
                return new DeleteBrandResponse { StatusCode = 404, Success = false, UserMessage = "Brand not found." };

            brand.IsDeleted = true;
            brand.DeleteDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(brand);
            await _db.SaveChangesAsync(cancellationToken);

            return new DeleteBrandResponse { StatusCode = 200, Success = true, UserMessage = "Brand deleted successfully." };
        }
    }
}
