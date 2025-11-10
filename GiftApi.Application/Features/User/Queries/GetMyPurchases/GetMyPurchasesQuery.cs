using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.User.Queries.GetMyPurchases
{
    public class GetMyPurchasesQuery : IRequest<GetMyPurchasesResponse>
    {
        public bool IncludeVoucher { get; set; } = true;
        public bool? IsUsed { get; set; }
        public PaginationModel Pagination { get; set; } = new PaginationModel();
    }
}
