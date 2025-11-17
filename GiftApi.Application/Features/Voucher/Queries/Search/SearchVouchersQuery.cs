using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.Search
{
    public class SearchVouchersQuery : IRequest<SearchVouchersResponse>
    {
        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Term { get; set; }
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}