using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Voucher.Queries.GetAll
{
    public class GetAllVouchersHandler : IRequestHandler<GetAllVouchersQuery, GetAllVouchersResponse>
    {
        readonly IVoucherRepository _voucherRepository;
        public GetAllVouchersHandler(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<GetAllVouchersResponse> Handle(GetAllVouchersQuery request, CancellationToken cancellationToken)
        {
            var vouchers = await _voucherRepository.GetAllWithCategoryAndBrand();

            if (vouchers == null || !vouchers.Any())
            {
                return new GetAllVouchersResponse
                {
                    Items = new List<GetAllVouchersItemsResponse>(),
                    TotalCount = 0,
                    Success = true,
                    StatusCode = 200
                };
            }
            var totalCount = vouchers.Count();
            var items = vouchers
                    .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(v => new GetAllVouchersItemsResponse
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
                        Brand = v.Brand != null && !v.IsDeleted ? new BrandDto
                        {
                            Id = v.Brand.Id,
                            Name = v.Brand.Name,
                            Description = v.Brand.Description,
                            LogoUrl = v.Brand.LogoUrl,
                            Website = v.Brand.Website,
                            CategoryId = v.Brand.CategoryId
                        } : null,
                        Category = v.Brand?.Category != null && !v.Brand.Category.IsDeleted ? new CategoryDto
                        {
                            Id = v.Brand.Category.Id,
                            Name = v.Brand.Category.Name, 
                            Logo = v.Brand.Category.Description,
                            Description = v.Brand.Category.Description,
                            CreateDate = v.Brand.Category.CreateDate,
                            UpdateDate = v.Brand.Category.UpdateDate
                        } : null
                    })
                    .ToList();

            return new GetAllVouchersResponse
            {
                TotalCount = totalCount,
                Items = items,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
