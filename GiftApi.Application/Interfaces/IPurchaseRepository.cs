using GiftApi.Application.DTOs;
using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<VoucherDeliveryInfo?> Get(Guid id);
        Task<List<VoucherDeliveryInfo>?> GetAll();
        Task<bool> RedeemAsync(Guid deliveryInfoId, Guid performedByUserId);
        Task<bool> UndoRedeemAsync(Guid deliveryInfoId, Guid performedByUserId);
    }
}
