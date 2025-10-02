using GiftApi.Application.Brand.DTOs;
using GiftApi.Application.Manage.Commands.UpdateVoucher;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Manage.Commands.EditVoucher
{
    public class UpdateVoucherHandler : IRequestHandler<UpdateVoucherCommand, UpdateVoucherResponse>
    {
        private readonly ApplicationDbContext _db;

        public UpdateVoucherHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UpdateVoucherResponse> Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

            if (voucher == null)
            {
                return new UpdateVoucherResponse
                {
                    Success = false,
                    UserMessage = $"Voucher with Id {request.Id} not found.",
                    StatusCode = 404
                };
            }

            if (request.BrandId.HasValue)
            {
                var brandExists = await _db.Brands
                    .AnyAsync(b => b.Id == request.BrandId.Value && !b.IsDeleted, cancellationToken);

                if (!brandExists)
                {
                    return new UpdateVoucherResponse
                    {
                        Success = false,
                        UserMessage = $"Brand with Id {request.BrandId.Value} does not exist or is deleted.",
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
            voucher.UpdateDate = DateTime.UtcNow;
            voucher.ImageUrl = request.ImageUrl;

            _db.Vouchers.Update(voucher);
            await _db.SaveChangesAsync(cancellationToken);

            BrandDto? brandDto = null;
            if (voucher.BrandId.HasValue)
            {
                brandDto = await _db.Brands
                    .Where(b => b.Id == voucher.BrandId.Value)
                    .Select(b => new BrandDto
                    {
                        BrandId = b.Id,
                        Name = b.Name
                    })
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new UpdateVoucherResponse
            {
                Success = true,
                UserMessage = "Voucher updated successfully",
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
