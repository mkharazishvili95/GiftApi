using GiftApi.Application.Brand.DTOs;
using GiftApi.Application.Category.DTOs;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Voucher.Queries.Get
{
    public class GetVoucherHandler : IRequestHandler<GetVoucherQuery, GetVoucherResponse>
    {
        readonly ApplicationDbContext _db;
        public GetVoucherHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<GetVoucherResponse> Handle(GetVoucherQuery request, CancellationToken cancellationToken)
        {
            if(request.Id == Guid.Empty)
                return new GetVoucherResponse { Id = Guid.Empty, Success = false, UserMessage = "Id is required.", StatusCode = 400 };

            var voucher = await _db.Vouchers
            .AsNoTracking()
            .Include(v => v.Brand)
            .ThenInclude(b => b.Category)
            .FirstOrDefaultAsync(v => v.Id == request.Id);

            if (voucher == null)
                return new GetVoucherResponse { Id = request.Id, Success = false, UserMessage = $"Voucher with Id {request.Id} not found.", StatusCode = 404 };

            if(voucher.IsDeleted)
                return new GetVoucherResponse { Id = request.Id, Success = false, UserMessage = $"Voucher with Id {request.Id} is deleted.", StatusCode = 404 };

            BrandDto? brandDto = null;
            if (voucher.Brand != null && !voucher.Brand.IsDeleted)
            {
                brandDto = new BrandDto
                {
                    BrandId = voucher.Brand.Id,
                    Name = voucher.Brand.Name,
                    Description = voucher.Brand.Description,
                    LogoUrl = voucher.Brand.LogoUrl,
                    Website = voucher.Brand.Website,
                    IsDeleted = voucher.Brand.IsDeleted,
                    CreateDate = voucher.Brand.CreateDate,
                    UpdateDate = voucher.Brand.UpdateDate,
                    DeleteDate = voucher.Brand.DeleteDate
                };
            }

            CategoryDto? categoryDto = null;
            if (brandDto != null && voucher.Brand?.Category != null && !voucher.Brand.Category.IsDeleted)
            {
                categoryDto = new CategoryDto
                {
                    CategoryId = voucher.Brand.Category.Id,
                    Name = voucher.Brand.Category.Name,
                    Description = voucher.Brand.Category.Description,
                    IsDeleted = voucher.Brand.Category.IsDeleted,
                    CreateDate = voucher.Brand.Category.CreateDate,
                    UpdateDate = voucher.Brand.Category.UpdateDate,
                    DeleteDate = voucher.Brand.Category.DeleteDate,
                    Logo = voucher.Brand.Category.Logo
                };
            }

            var response = new GetVoucherResponse
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

            return response;
        }
    }
}
