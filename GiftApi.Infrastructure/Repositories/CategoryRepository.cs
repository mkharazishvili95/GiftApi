using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        readonly ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Category?> Create(Category? category)
        {
            var newCategory = new GiftApi.Domain.Entities.Category
            {
                Name = category.Name,
                Description = category.Description,
                Logo = category.Logo,
                CreateDate = DateTime.UtcNow.AddHours(4),
                IsDeleted = false,
                DeleteDate = null,
                UpdateDate = null
            };

            _db.Categories.Add(category);

            return newCategory;
        }

        public async Task<Category?> Get(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task<Category?> Edit(Category? category)
        {
            var existingCategory = await _db.Categories.FindAsync(category.Id);

            if (existingCategory == null)
                return null;

            if (existingCategory.IsDeleted)
                return null;

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Logo = category.Logo;
            existingCategory.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(existingCategory);
            return existingCategory;
        }

        public async Task<bool> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return false;

            if (category.IsDeleted)
                return false;

            category.IsDeleted = true;
            category.DeleteDate = DateTime.UtcNow.AddHours(4);
            _db.Categories.Update(category);

            return true;
        }

        public async Task<bool> Restore(int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
                return false;

            if (!category.IsDeleted)
                return false;

            category.IsDeleted = false;
            category.DeleteDate = null;
            category.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(category);
            return true;
        }

        public async Task<bool> CategoryExists(int categoryId)
        {
            return await _db.Categories.AnyAsync(x => x.Id == categoryId && !x.IsDeleted);
        }

        public async Task<Category?> GetWithBrands(int id)
        {
            return await _db.Categories
                .Include(c => c.Brands.Where(b => !b.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<List<Category>?> GetAllCategoriesWithBrandsAsync(CancellationToken cancellationToken)
        {
           return await _db.Categories
                .Include(c => c.Brands.Where(b => !b.IsDeleted))
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Category>?> GetAll(CancellationToken cancellationToken)
        {
            return await _db.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }
    }
}
