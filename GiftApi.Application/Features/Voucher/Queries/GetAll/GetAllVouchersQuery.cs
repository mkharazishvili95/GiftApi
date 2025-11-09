using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.GetAll
{
    public class GetAllVouchersQuery : IRequest<GetAllVouchersResponse>
    {
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
