using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;

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

            if(existingCategory.IsDeleted)
                return null;

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Logo = category.Logo;
            existingCategory.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(existingCategory);
            return existingCategory;
        }
    }
}
