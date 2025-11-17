using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Delete
{
    public class DeleteVoucherHandler : IRequestHandler<DeleteVoucherCommand, DeleteVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IUnitOfWork _unitOfWork;

        public DeleteVoucherHandler(IVoucherRepository voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteVoucherResponse> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return new DeleteVoucherResponse { Success = false, StatusCode = 400, Message = "Voucher Id is required." };

            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null)
                return new DeleteVoucherResponse { Success = false, StatusCode = 404, Message = "Voucher not found." };

            if (voucher.IsDeleted)
                return new DeleteVoucherResponse { Success = false, StatusCode = 400, Message = "Voucher already deleted." };

            var result = await _voucherRepository.Delete(request.Id);
            if (!result)
                return new DeleteVoucherResponse { Success = false, StatusCode = 500, Message = "Failed to delete voucher." };

            await _unitOfWork.SaveChangesAsync();

            return new DeleteVoucherResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Voucher deleted successfully.",
                Id = request.Id
            };
        }
    }
}