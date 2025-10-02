using GiftApi.Application.Brand.DTOs;
using GiftApi.Core.Entities;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Manage.Commands.CreateVoucher
{
    public class CreateVoucherHandler : IRequestHandler<CreateVoucherCommand, CreateVoucherResponse>
    {
        private readonly ApplicationDbContext _db;

        public CreateVoucherHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreateVoucherResponse> Handle(CreateVoucherCommand request, CancellationToken cancellationToken)
        {
            if (request.BrandId.HasValue)
            {
                var brandExists = await _db.Brands
                    .AnyAsync(b => b.Id == request.BrandId.Value && !b.IsDeleted, cancellationToken);

                if (!brandExists)
                    return new CreateVoucherResponse {  Success = false,  UserMessage = $"Brand with Id {request.BrandId.Value} does not exist or it's deleted.", StatusCode = 400 };
                
            }

            var voucher = new GiftApi.Core.Entities.Voucher
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
                CreateDate = DateTime.UtcNow,
                UpdateDate = request.UpdateDate,
                ImageUrl = request.ImageUrl,
            };

            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync(cancellationToken);

            BrandDto? brandDto = null;
            if (voucher.BrandId.HasValue)
            {
                var brand = await _db.Brands
                    .Where(b => b.Id == voucher.BrandId.Value)
                    .Select(b => new BrandDto
                    {
                        BrandId = b.Id,
                        Name = b.Name
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                brandDto = brand;
            }

            return new CreateVoucherResponse
            {
                Success = true,
                UserMessage = "Voucher created successfully",
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
