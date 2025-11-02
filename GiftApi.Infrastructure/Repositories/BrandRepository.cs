using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Brand?> Get(int id)
        {
           return await _db.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Brand?> Update(Brand? brand)
        {
            var existingBrand = await _db.Brands.FirstOrDefaultAsync(x => x.Id == brand.Id);

            if (existingBrand == null)
                return null;

            existingBrand.Name = brand.Name;
            existingBrand.Description = brand.Description;
            existingBrand.LogoUrl = brand.LogoUrl;
            existingBrand.Website = brand.Website;
            existingBrand.CategoryId = brand.CategoryId;
            existingBrand.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(existingBrand);
            return existingBrand;
        }
    }
}
