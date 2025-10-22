using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;

namespace GiftApi.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
    }
}
