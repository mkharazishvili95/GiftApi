using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Activate
{
    public class ActivateVoucherHandler : IRequestHandler<ActivateVoucherCommand, ActivateVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IUnitOfWork _unitOfWork;

        public ActivateVoucherHandler(IVoucherRepository voucherRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ActivateVoucherResponse> Handle(ActivateVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _voucherRepository.GetByIdAsync(request.Id);
            if (voucher == null) 
                return new ActivateVoucherResponse {  Success = false,  StatusCode = 404, Message = "Voucher not found." }; 

            if (voucher.IsActive == request.IsActive)
            {
                return new ActivateVoucherResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Id = voucher.Id,
                    IsActive = voucher.IsActive,
                    Message = $"Already {(voucher.IsActive ? "active" : "deactivated")}."
                };
            }

            voucher.IsActive = request.IsActive;
            voucher.UpdateDate = DateTime.UtcNow.AddHours(4);

            await _voucherRepository.Edit(voucher);
            await _unitOfWork.SaveChangesAsync();

            return new ActivateVoucherResponse
            {
                Success = true,
                StatusCode = 200,
                Id = voucher.Id,
                IsActive = voucher.IsActive,
                Message = voucher.IsActive ? "Activated." : "Deactivated."
            };
        }
    }
}