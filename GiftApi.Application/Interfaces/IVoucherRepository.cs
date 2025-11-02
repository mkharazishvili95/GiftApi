namespace GiftApi.Application.Interfaces
{
    public interface IVoucherRepository
    {
        Task<Domain.Entities.Voucher?> Create(Domain.Entities.Voucher? voucher);
    }
}
