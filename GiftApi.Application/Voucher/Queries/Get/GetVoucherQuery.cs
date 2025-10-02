using MediatR;

namespace GiftApi.Application.Voucher.Queries.Get
{
    public class GetVoucherQuery : IRequest<GetVoucherResponse>
    {
        public Guid Id { get; set; }
    }
}
