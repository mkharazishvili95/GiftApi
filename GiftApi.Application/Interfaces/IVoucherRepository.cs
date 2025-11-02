namespace GiftApi.Application.Interfaces
{
    public interface IVoucherRepository
    {
        Task<Domain.Entities.Voucher?> Create(Domain.Entities.Voucher? voucher);
        Task<Domain.Entities.Voucher?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Voucher?> Edit(Domain.Entities.Voucher? voucher);
    }
}
