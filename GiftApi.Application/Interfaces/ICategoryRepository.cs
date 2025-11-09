using GiftApi.Application.DTOs;
using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> Create(Category? category);
        Task<Category?> Get(int id);
        Task<Category?> Edit(Category? category);
        Task<bool> Delete(int id);
        Task<bool> Restore(int id);
        Task<bool> CategoryExists(int categoryId);
        Task<Category?> GetWithBrands(int id);
        Task<List<Category>?> GetAllCategoriesWithBrandsAsync(CancellationToken cancellationToken);
    }
}
