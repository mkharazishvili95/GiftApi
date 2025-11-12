using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Export
{
    public class ExportAllPurchasesQuery : IRequest<ExportAllPurchasesResponse>
    {
        public Guid? SenderId { get; set; }
        public Guid? VoucherId { get; set; }
        public bool? IsUsed { get; set; }
        public string? SearchString { get; set; }
    }
}