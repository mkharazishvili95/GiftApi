using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Edit
{
    public class EditVoucherHandler : IRequestHandler<EditVoucherCommand, EditVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        readonly IBrandRepository _brandRepository;
        readonly IUnitOfWork _unitOfWork;

        public EditVoucherHandler(IVoucherRepository voucherRepository, IBrandRepository brandRepository, IUnitOfWork unitOfWork)
        {
            _voucherRepository = voucherRepository;
            _brandRepository = brandRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EditVoucherResponse> Handle(EditVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher =  await _voucherRepository.GetByIdAsync(request.Id);

            if (voucher == null)
            {
                return new EditVoucherResponse
                {
                    Success = false,
                    Message = $"Voucher with Id {request.Id} not found.",
                    StatusCode = 404
                };
            }

            if (request.BrandId.HasValue)
            {
                var brandExists = await _brandRepository.BrandExists(request.BrandId.Value);

                if (!brandExists)
                {
                    return new EditVoucherResponse
                    {
                        Success = false,
                        Message = $"Brand with Id {request.BrandId.Value} does not exist or is deleted.",
                        StatusCode = 400
                    };
                }
            }

            voucher.Title = request.Title;
            voucher.Description = request.Description;
            voucher.Amount = request.Amount;
            voucher.IsPercentage = request.IsPercentage;
            voucher.BrandId = request.BrandId;
            voucher.ValidMonths = request.ValidMonths;
            voucher.Unlimited = request.Unlimited;
            voucher.Quantity = request.Quantity;
            voucher.Redeemed = request.Redeemed;
            voucher.IsActive = request.IsActive;
            voucher.UpdateDate = DateTime.UtcNow.AddHours(4);
            voucher.ImageUrl = request.ImageUrl;

            await _voucherRepository.Edit(voucher);
            await _unitOfWork.SaveChangesAsync();

            BrandDto? brandDto = null;
            if (voucher.BrandId.HasValue)
            {
                brandDto = await _brandRepository.GetBrandDtoByIdAsync(voucher.BrandId.Value, cancellationToken);
            }

            return new EditVoucherResponse
            {
                Success = true,
                Message = "Voucher edited successfully",
                StatusCode = 200,
                Id = voucher.Id,
                Title = voucher.Title,
                Description = voucher.Description,
                Amount = voucher.Amount,
                IsPercentage = voucher.IsPercentage,
                BrandId = voucher.BrandId,
            };
        }
    }
}
