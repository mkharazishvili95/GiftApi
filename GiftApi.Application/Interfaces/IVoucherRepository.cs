using GiftApi.Domain.Entities;

namespace GiftApi.Application.Interfaces
{
    public interface IVoucherRepository
    {
        Task<Domain.Entities.Voucher?> Create(Domain.Entities.Voucher? voucher);
        Task<Domain.Entities.Voucher?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Voucher?> Edit(Domain.Entities.Voucher? voucher);
        Task<Voucher?> GetWithCategoryAndBrand(Guid id);
        Task<List<Voucher>?> GetAllWithCategoryAndBrand();
        Task<VoucherDeliveryInfo?> GetDeliveryInfoByIdAsync(Guid id);
        Task<bool> Buy(Guid voucherId, Guid userId, int quantity, string recipientName, string recipientPhone, string recipientCity, string recipientAddress, string? recipientEmail = null, string? message = null, string? senderName = null, Guid? senderId = null);
        Task<List<VoucherDeliveryInfo>> GetDeliveryInfosBySenderAsync(Guid senderId, bool includeVoucher, bool? isUsedFilter, int page, int pageSize);
        Task<bool> Delete(Guid id);
        Task<bool> Restore(Guid id);
    }
}
