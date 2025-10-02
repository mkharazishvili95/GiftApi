using GiftApi.Common.Models;
using MediatR;

namespace GiftApi.Application.Voucher.Queries.GetAll
{
    public class GetAllVouchersQuery : IRequest<GetAllVouchersResponse>
    {
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
        public int ? BrandId { get; set; }
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
