using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.Get
{
    public class GetVoucherHandler : IRequestHandler<GetVoucherQuery, GetVoucherResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        public GetVoucherHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<GetVoucherResponse> Handle(GetVoucherQuery request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                return new GetVoucherResponse { Id = Guid.Empty, Success = false, Message = "Id is required.", StatusCode = 400 };

            var voucher = await _voucherRepository.GetWithCategoryAndBrand(request.Id);

            if (voucher == null)
                return new GetVoucherResponse { Id = request.Id, Success = false, Message = $"Voucher with Id {request.Id} not found.", StatusCode = 404 };

            if (voucher.IsDeleted)
                return new GetVoucherResponse { Id = request.Id, Success = false, Message = $"Voucher with Id {request.Id} is deleted.", StatusCode = 404 };

            if (!voucher.IsActive)
                return new GetVoucherResponse { Id = request.Id, Success = false, Message = $"Voucher with Id {request.Id} not found.", StatusCode = 404 };

            var brandDto = (voucher.Brand != null && !voucher.Brand.IsDeleted)
            ? new BrandDto
            {
                Id = voucher.Brand.Id,
                Name = voucher.Brand.Name,
                Description = voucher.Brand.Description,
                LogoUrl = voucher.Brand.LogoUrl,
                Website = voucher.Brand.Website
            }
            : null;

            var categoryDto = (brandDto != null && voucher.Brand?.Category != null && !voucher.Brand.Category.IsDeleted)
                ? new CategoryDto
                {
                    Id = voucher.Brand.Category.Id,
                    Name = voucher.Brand.Category.Name,
                    Description = voucher.Brand.Category.Description,
                    IsDeleted = voucher.Brand.Category.IsDeleted,
                    CreateDate = voucher.Brand.Category.CreateDate,
                    UpdateDate = voucher.Brand.Category.UpdateDate,
                    DeleteDate = voucher.Brand.Category.DeleteDate,
                    Logo = voucher.Brand.Category.Logo
                }
                : null;


            return new GetVoucherResponse
            {
                Id = voucher.Id,
                Title = voucher.Title,
                Description = voucher.Description,
                Amount = voucher.Amount,
                IsPercentage = voucher.IsPercentage,
                ValidMonths = voucher.ValidMonths,
                Unlimited = voucher.Unlimited,
                Quantity = voucher.Quantity,
                Redeemed = voucher.Redeemed,
                IsActive = voucher.IsActive,
                CreateDate = voucher.CreateDate,
                UpdateDate = voucher.UpdateDate,
                ImageUrl = voucher.ImageUrl,
                Brand = brandDto,
                Category = categoryDto,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
