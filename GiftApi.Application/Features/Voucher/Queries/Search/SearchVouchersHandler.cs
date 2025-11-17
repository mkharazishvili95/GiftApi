using GiftApi.Application.Features.Voucher.Queries.Search;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.Search
{
    public class SearchVouchersHandler : IRequestHandler<SearchVouchersQuery, SearchVouchersResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        public SearchVouchersHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<SearchVouchersResponse> Handle(SearchVouchersQuery request, CancellationToken cancellationToken)
        {
            if (request.MinAmount.HasValue &&
                request.MaxAmount.HasValue &&
                request.MinAmount.Value > request.MaxAmount.Value)
            {
                return new SearchVouchersResponse
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "minAmount cannot be greater than maxAmount",
                    Items = new(),
                    TotalCount = 0
                };
            }

            var vouchers = await _voucherRepository.SearchAsync(
                request.BrandId,
                request.CategoryId,
                request.MinAmount,
                request.MaxAmount,
                request.Term
            );

            if (vouchers == null || vouchers.Count == 0)
            {
                return new SearchVouchersResponse
                {
                    Success = true,
                    StatusCode = 200,
                    Items = new(),
                    TotalCount = 0
                };
            }

            var total = vouchers.Count;

            var page = request.Pagination.Page < 1 ? 1 : request.Pagination.Page;
            var pageSize = request.Pagination.PageSize <= 0 ? 10 : request.Pagination.PageSize;

            var pageItems = vouchers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new SearchVouchersItemResponse
                {
                    Id = v.Id,
                    Title = v.Title,
                    Description = v.Description,
                    Amount = v.Amount,
                    IsPercentage = v.IsPercentage,
                    BrandId = v.BrandId,
                    CategoryId = v.Brand?.CategoryId,
                    ValidMonths = v.ValidMonths,
                    Unlimited = v.Unlimited,
                    Quantity = v.Quantity,
                    Redeemed = v.Redeemed,
                    IsActive = v.IsActive,
                    CreateDate = v.CreateDate,
                    UpdateDate = v.UpdateDate,
                    ImageUrl = v.ImageUrl,
                    Brand = v.Brand != null && !v.Brand.IsDeleted ? new Application.DTOs.BrandDto
                    {
                        Id = v.Brand.Id,
                        Name = v.Brand.Name,
                        Description = v.Brand.Description,
                        LogoUrl = v.Brand.LogoUrl,
                        Website = v.Brand.Website,
                        CategoryId = v.Brand.CategoryId
                    } : null,
                    Category = v.Brand?.Category != null && !v.Brand.Category.IsDeleted ? new Application.DTOs.CategoryDto
                    {
                        Id = v.Brand.Category.Id,
                        Name = v.Brand.Category.Name,
                        Description = v.Brand.Category.Description,
                        Logo = v.Brand.Category.Logo,
                        CreateDate = v.Brand.Category.CreateDate,
                        UpdateDate = v.Brand.Category.UpdateDate
                    } : null
                })
                .ToList();

            return new SearchVouchersResponse
            {
                Success = true,
                StatusCode = 200,
                TotalCount = total,
                Items = pageItems
            };
        }
    }
}