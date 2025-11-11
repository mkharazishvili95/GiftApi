using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.GetAll
{
    public class GetAllPurchasesQuery : IRequest<GetAllPurchasesResponse>
    {
        public PaginationModel Pagination { get; set; } = new();
        public Guid? SenderId { get; set; }
        public Guid? VoucherId { get; set; }
        public bool? IsUsed { get; set; }
        public string? SearchString { get; set; }
    }
}
