using GiftApi.Application.DTOs;
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

        public async Task<bool> BrandExists(int id)
        {
            return await _db.Brands.AnyAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<BrandDto?> GetBrandDtoByIdAsync(int brandId, CancellationToken cancellationToken)
        {
            return await _db.Brands
                .Where(b => b.Id == brandId)
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<Brand> GetQueryable()
        {
            return _db.Brands.AsNoTracking();
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

        public async Task<bool> Delete(int id)
        {
            var brand = await _db.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (brand == null)
                return false;

            if(brand.IsDeleted)
                return false;

            brand.IsDeleted = true;
            brand.DeleteDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(brand);
            return true;
        }

        public async Task<Brand?> Get(int id)
        {
           return await _db.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> Restore(int id)
        {
            var brand = await Get(id);

            if (brand == null)
                return false;

            if (!brand.IsDeleted)
                return false;

            brand.IsDeleted = false;
            brand.DeleteDate = null;
            brand.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Brands.Update(brand);
            return true;
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
