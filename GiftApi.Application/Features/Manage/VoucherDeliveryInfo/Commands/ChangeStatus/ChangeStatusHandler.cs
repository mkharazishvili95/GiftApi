using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.ChangeStatus
{
    public class ChangeStatusHandler : IRequestHandler<ChangeStatusCommand, ChangeStatusResponse>
    {
        readonly IManageRepository _manageRepository;
        public ChangeStatusHandler(IManageRepository manageRepository)
        {
            _manageRepository = manageRepository;
        }
        public async Task<ChangeStatusResponse> Handle(ChangeStatusCommand request, CancellationToken cancellationToken)
        {
            if (request.DeliveryInfoId == Guid.Empty)
                return new ChangeStatusResponse { StatusCode = 400, Success = false, Message = "Id is required." };

            var result = await _manageRepository.ChangeUsedStatus(request.DeliveryInfoId);
            if (!result)
                return new ChangeStatusResponse { StatusCode = 404, Success = false, Message = "Delivery info not found or already used." };

            return new ChangeStatusResponse { StatusCode = 200, Success = true, Message = "Delivery info status changed to used." };
        }
    }
}