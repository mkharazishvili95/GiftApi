using GiftApi.Application.Brand.DTOs;
using GiftApi.Application.Category.DTOs;
using GiftApi.Application.Voucher.Queries.GetAll;
using GiftApi.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GiftApi.Application.Voucher.Queries.GetAll
{
    public class GetAllVouchersHandler : IRequestHandler<GetAllVouchersQuery, GetAllVouchersResponse>
    {
        private readonly ApplicationDbContext _db;
        public GetAllVouchersHandler(ApplicationDbContext db) => _db = db;

        public async Task<GetAllVouchersResponse> Handle(GetAllVouchersQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Vouchers
                .AsNoTracking()
                .Include(v => v.Brand)
                .ThenInclude(b => b.Category)
                .Where(v => !v.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchString))
                query = query.Where(v => v.Title.ToLower().Contains(request.SearchString.Trim().ToLower())
                                       || v.Description.ToLower().Contains(request.SearchString.Trim().ToLower()));

            if (request.CategoryId.HasValue)
                query = query.Where(v => v.Brand.Category != null
                                        && v.Brand.Category.Id == request.CategoryId.Value
                                        && !v.Brand.Category.IsDeleted);

            if (request.BrandId.HasValue)
                query = query.Where(v => v.Brand != null
                                        && v.Brand.Id == request.BrandId.Value
                                        && !v.Brand.IsDeleted);

            var totalCount = await query.CountAsync(cancellationToken);

            var vouchers = await query
                .OrderByDescending(v => v.CreateDate)
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync(cancellationToken);

            var responseItems = vouchers.Select(v => new GetAllVouchersItemsResponse
            {
                Id = v.Id,
                Title = v.Title,
                Description = v.Description,
                Amount = v.Amount,
                IsPercentage = v.IsPercentage,
                ValidMonths = v.ValidMonths,
                Unlimited = v.Unlimited,
                Quantity = v.Quantity,
                Redeemed = v.Redeemed,
                IsActive = v.IsActive,
                CreateDate = v.CreateDate,
                UpdateDate = v.UpdateDate,
                ImageUrl = v.ImageUrl,
                Brand = (v.Brand != null && !v.Brand.IsDeleted) ? new BrandDto
                {
                    BrandId = v.Brand.Id,
                    Name = v.Brand.Name,
                    Description = v.Brand.Description,
                    LogoUrl = v.Brand.LogoUrl,
                    Website = v.Brand.Website,
                    IsDeleted = v.Brand.IsDeleted,
                    CreateDate = v.Brand.CreateDate,
                    UpdateDate = v.Brand.UpdateDate,
                    DeleteDate = v.Brand.DeleteDate
                } : null,
                Category = (v.Brand?.Category != null && !v.Brand.Category.IsDeleted) ? new CategoryDto
                {
                    CategoryId = v.Brand.Category.Id,
                    Name = v.Brand.Category.Name,
                    Description = v.Brand.Category.Description,
                    IsDeleted = v.Brand.Category.IsDeleted,
                    CreateDate = v.Brand.Category.CreateDate,
                    UpdateDate = v.Brand.Category.UpdateDate,
                    DeleteDate = v.Brand.Category.DeleteDate,
                    Logo = v.Brand.Category.Logo
                } : null
            }).ToList();

            return new GetAllVouchersResponse
            {
                TotalCount = totalCount,
                Vouchers = responseItems,
                StatusCode = 200,
                Success = true
            };
        }
    }
}
