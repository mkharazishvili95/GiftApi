using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.User.Queries.GetMyPurchases
{
    public class GetMyPurchasesHandler : IRequestHandler<GetMyPurchasesQuery, GetMyPurchasesResponse>
    {
        readonly ICurrentUserRepository _currentUserRepository;
        readonly IVoucherRepository _voucherRepository;

        public GetMyPurchasesHandler(ICurrentUserRepository currentUserRepository, IVoucherRepository voucherRepository)
        {
            _currentUserRepository = currentUserRepository;
            _voucherRepository = voucherRepository;
        }

        public async Task<GetMyPurchasesResponse> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserRepository.GetUserId();
            if (!userId.HasValue)
                return new GetMyPurchasesResponse  { Success = false, StatusCode = 401, Message = "Unauthorized." };

            int page = request.Pagination?.Page <= 0 ? 1 : request.Pagination.Page;
            int pageSize = request.Pagination?.PageSize <= 0 ? 10 : request.Pagination.PageSize;

            var items = await _voucherRepository.GetDeliveryInfosBySenderAsync(
                userId.Value,
                request.IncludeVoucher,
                request.IsUsed,
                page,
                pageSize
            );

            var responseItems = items.Select(x => new GetMyPurchasesPurchasesItemsResponse
            {
                Id = x.Id,
                VoucherId = x.VoucherId,
                SenderName = x.SenderName,
                RecipientName = x.RecipientName,
                RecipientEmail = x.RecipientEmail,
                RecipientPhone = x.RecipientPhone,
                RecipientCity = x.RecipientCity,
                RecipientAddress = x.RecipientAddress,
                Message = x.Message,
                Quantity = x.Quantity,
                IsUsed = x.IsUsed,
                Brand = request.IncludeVoucher && x.Voucher?.Brand != null
                    ? new BrandDto
                    {
                        Id = x.Voucher.Brand.Id,
                        Name = x.Voucher.Brand.Name,
                        Description = x.Voucher.Brand.Description,
                        LogoUrl = x.Voucher.Brand.LogoUrl,
                        Website = x.Voucher.Brand.Website,
                        CategoryId = x.Voucher.Brand.CategoryId
                    }
                    : null,
                Category = request.IncludeVoucher && x.Voucher?.Brand?.Category != null
                    ? new CategoryDto
                    {
                        Id = x.Voucher.Brand.Category.Id,
                        Name = x.Voucher.Brand.Category.Name,
                        Description = x.Voucher.Brand.Category.Description,
                        IsDeleted = x.Voucher.Brand.Category.IsDeleted,
                        DeleteDate = x.Voucher.Brand.Category.DeleteDate,
                        CreateDate = x.Voucher.Brand.Category.CreateDate,
                        UpdateDate = x.Voucher.Brand.Category.UpdateDate,
                        Logo = x.Voucher.Brand.Category.Logo
                    }
                    : null
            }).ToList();
            var response = new GetMyPurchasesResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "OK",
                Items = responseItems,
                TotalCount = responseItems.Count 
            };

            return response;
        }
    }
}