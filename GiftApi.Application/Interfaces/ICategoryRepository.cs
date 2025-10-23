using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> Create(Category? category);
        Task<Category?> Get(int id);
        Task<Category?> Edit(Category? category);
    }
}
