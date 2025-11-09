using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.Get
{
    public class GetVoucherQuery : IRequest<GetVoucherResponse>
    {
        public Guid Id { get; set; }
    }
}
