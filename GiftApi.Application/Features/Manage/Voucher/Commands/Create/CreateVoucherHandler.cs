using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Voucher.Commands.Create
{
    public class CreateVoucherHandler : IRequestHandler<CreateVoucherCommand, CreateVoucherResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IVoucherRepository _voucherRepository;
        readonly IBrandRepository _brandRepository;

        public CreateVoucherHandler(IUnitOfWork unitOfWork, IVoucherRepository voucherRepository, IBrandRepository brandRepository)
        {
            _unitOfWork = unitOfWork;
            _voucherRepository = voucherRepository;
            _brandRepository = brandRepository;
        }

        public async Task<CreateVoucherResponse> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {
            if (request.BrandId.HasValue)
            {
                var brandExists = await _brandRepository.BrandExists(request.BrandId.Value);

                if (!brandExists)
                    return new CreateVoucherResponse { Success = false, Message = $"Brand with Id {request.BrandId.Value} does not exist or it's deleted.", StatusCode = 400 };

            }

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Amount = request.Amount,
                IsPercentage = request.IsPercentage,
                BrandId = request.BrandId,
                ValidMonths = request.ValidMonths,
                Unlimited = request.Unlimited,
                Quantity = request.Quantity,
                Redeemed = request.Redeemed,
                IsActive = request.IsActive,
                CreateDate = DateTime.UtcNow.AddHours(4),
                UpdateDate = null,
                ImageUrl = request.ImageUrl,
            };

            await _voucherRepository.Create(voucher);
            await _unitOfWork.SaveChangesAsync();

            BrandDto? brandDto = null;
            if (voucher.BrandId.HasValue)
            {
                brandDto = await _brandRepository.GetBrandDtoByIdAsync(voucher.BrandId.Value, cancellationToken);
            }

            return new CreateVoucherResponse
            {
                Success = true,
                Message = "Voucher created successfully",
                StatusCode = 200,
                Id = voucher.Id,
                Title = voucher.Title,
                Description = voucher.Description,
                Amount = voucher.Amount,
                IsPercentage = voucher.IsPercentage,
                BrandId = voucher.BrandId
            };
        }
    }
}
