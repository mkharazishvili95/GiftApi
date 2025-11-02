using System.Threading;

namespace GiftApi.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<Domain.Entities.Brand?> Create(Domain.Entities.Brand? brand);
    }
}
