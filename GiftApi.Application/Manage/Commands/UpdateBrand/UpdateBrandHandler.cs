using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Manage.Commands.UpdateBrand
{
    public class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand, UpdateBrandResponse>
    {
        readonly ApplicationDbContext _db;
        public UpdateBrandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UpdateBrandResponse> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new UpdateBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand Id is required." };

            var brand = await _db.Brands.FindAsync(request.Id);

            if (brand == null || brand.IsDeleted)
                return new UpdateBrandResponse { StatusCode = 404, Success = false, UserMessage = "Brand not found." };

            if (string.IsNullOrEmpty(request.Name))
                return new UpdateBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand name is required." };

            if (request.CategoryId.HasValue)
            {
                if (request.CategoryId <= 0)
                    return new UpdateBrandResponse { StatusCode = 400, Success = false, UserMessage = "CategoryId must be greater than zero." };

                var categoryExists = await _db.Categories
                    .AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken);

                if (!categoryExists)
                    return new UpdateBrandResponse { StatusCode = 404, Success = false, UserMessage = "Category not found." };

                brand.CategoryId = request.CategoryId.Value;
            }
            brand.Name = request.Name;
            brand.Description = request.Description;
            brand.LogoUrl = request.LogoUrl;
            brand.Website = request.Website;
            brand.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(brand);
            await _db.SaveChangesAsync(cancellationToken);

            return new UpdateBrandResponse
            {
                StatusCode = 200,
                Success = true,
                UserMessage = "Brand updated successfully."
            };
        }
    }
}
