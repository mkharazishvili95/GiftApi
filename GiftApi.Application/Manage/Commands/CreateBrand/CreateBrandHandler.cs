using GiftApi.Core.Entities;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Manage.Commands.CreateBrand
{
    public class CreateBrandHandler : IRequestHandler<CreateBrandCommand, CreateBrandResponse>
    {
        readonly ApplicationDbContext _db;
        public CreateBrandHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreateBrandResponse> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(request.Name))
                return new CreateBrandResponse { StatusCode = 400, Success = false, UserMessage = "Brand name is required." };

            if (request.CategoryId <= 0)
                return new CreateBrandResponse { StatusCode = 400, Success = false, UserMessage = "CategoryId is required" };


            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken);

            if (!categoryExists)
                return new CreateBrandResponse { StatusCode = 404, Success = false,  UserMessage = "Category not found."  };

            var brand = new Core.Entities.Brand
            {
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                CreateDate = DateTime.UtcNow.AddHours(4),
                DeleteDate = null,
                IsDeleted = false,
                UpdateDate = null,
                CategoryId = request.CategoryId
            };

            await _db.Brands.AddAsync(brand);
            await _db.SaveChangesAsync(cancellationToken);

            return new CreateBrandResponse { StatusCode = 200, Success = true, Id = brand.Id, Name = brand.Name, Description = brand.Description, LogoUrl = brand.LogoUrl, Website = brand.Website };
        }
    }
}
