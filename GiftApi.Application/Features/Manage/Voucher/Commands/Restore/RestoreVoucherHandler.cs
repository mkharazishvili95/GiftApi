using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Restore
{
    public class RestoreVoucherHandler : IRequestHandler<RestoreVoucherCommand, RestoreVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IUnitOfWork _unitOfWork;

        public RestoreVoucherHandler(IVoucherRepository voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RestoreVoucherResponse> Handle(RestoreVoucherCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return new RestoreVoucherResponse { Success = false, StatusCode = 400, Message = "Voucher Id is required." };

            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null)
                return new RestoreVoucherResponse { Success = false, StatusCode = 404, Message = "Voucher not found." };

            if (!voucher.IsDeleted)
                return new RestoreVoucherResponse { Success = false, StatusCode = 400, Message = "Voucher is not deleted." };

            var result = await _voucherRepository.Restore(request.Id);
            if (!result)
                return new RestoreVoucherResponse { Success = false, StatusCode = 500, Message = "Failed to restore voucher." };

            await _unitOfWork.SaveChangesAsync();

            return new RestoreVoucherResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Voucher restored successfully.",
                Id = request.Id
            };
        }
    }
}