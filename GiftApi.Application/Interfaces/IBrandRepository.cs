using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Domain.Entities.Brand?> Create(Domain.Entities.Brand? brand);
        Task<Domain.Entities.Brand?> Update(Domain.Entities.Brand? brand);
        Task<Brand?> Get(int id);
    }
}
