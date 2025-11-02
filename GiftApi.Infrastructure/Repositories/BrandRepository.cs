using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;

namespace GiftApi.Infrastructure.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        readonly ApplicationDbContext _db;
        public BrandRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Brand?> Create(Brand? brand)
        {
            var newBrand = new GiftApi.Domain.Entities.Brand
            {
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                CategoryId = brand.CategoryId,
                CreateDate = DateTime.UtcNow.AddHours(4),
                IsDeleted = false,
                DeleteDate = null,
                UpdateDate = null
            };
            _db.Brands.Add(brand);
            return newBrand;
        }
    }
}
