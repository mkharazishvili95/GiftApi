using GiftApi.Application.DTOs;
using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Domain.Entities.Brand?> Create(Domain.Entities.Brand? brand);
        Task<Domain.Entities.Brand?> Update(Domain.Entities.Brand? brand);
        Task<Brand?> Get(int id);
        Task<bool> Delete(int id);
        Task<bool> Restore(int id); 
        Task<bool> BrandExists(int id);
        Task<BrandDto?> GetBrandDtoByIdAsync(int brandId, CancellationToken cancellationToken);
        IQueryable<Brand> GetQueryable();
    }
}
