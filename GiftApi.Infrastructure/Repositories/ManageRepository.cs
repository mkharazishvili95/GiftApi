using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;

namespace GiftApi.Infrastructure.Repositories
{
    public class ManageRepository : IManageRepository
    {
        readonly ApplicationDbContext _db;
        public ManageRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> ChangeUsedStatus(Guid id)
        {
            var deliveryInfo = await _db.VoucherDeliveryInfos.FindAsync(id);
            if (deliveryInfo == null || deliveryInfo.IsUsed == true) return false;

            deliveryInfo.IsUsed = true;
            _db.VoucherDeliveryInfos.Update(deliveryInfo);
            await _db.SaveChangesAsync();

            return true;
        }
    }
}
