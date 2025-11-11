using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.GetAll
{
    public class GetAllPurchasesHandler : IRequestHandler<GetAllPurchasesQuery, GetAllPurchasesResponse>
    {
        private readonly IPurchaseRepository _purchaseRepository;

        public GetAllPurchasesHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<GetAllPurchasesResponse> Handle(GetAllPurchasesQuery request, CancellationToken cancellationToken)
        {
            var purchases = await _purchaseRepository.GetAll();

            if (purchases == null || !purchases.Any())
                return new GetAllPurchasesResponse { Success = true, Message = "No purchases found", TotalCount = 0, Items = new List<GetAllPurchasesItemsResponse>() };

            if (request.IsUsed.HasValue)
                purchases = purchases.Where(x => x.IsUsed == request.IsUsed.Value).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                string searchLower = request.SearchString.Trim().ToLower();
                purchases = purchases.Where(x =>
                    (!string.IsNullOrEmpty(x.RecipientName) && x.RecipientName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.RecipientEmail) && x.RecipientEmail.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.SenderName) && x.SenderName.ToLower().Contains(searchLower))
                ).ToList();
            }

            var totalCount = purchases.Count;

            if(totalCount == 0)
                return new GetAllPurchasesResponse { Success = true, Message = "No purchases found", TotalCount = 0, Items = new List<GetAllPurchasesItemsResponse>() };

            purchases = purchases
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToList();

            var items = purchases.Select(x => new GetAllPurchasesItemsResponse
            {
                Id = x.Id,
                VoucherId = x.VoucherId,
                SenderName = x.SenderName,
                RecipientName = x.RecipientName,
                RecipientEmail = x.RecipientEmail,
                RecipientPhone = x.RecipientPhone,
                RecipientCity = x.RecipientCity,
                RecipientAddress = x.RecipientAddress,
                SenderMessage = x.Message,
                SenderId = x.SenderId,
                Quantity = x.Quantity,
                IsUsed = x.IsUsed
            }).ToList();

            return new GetAllPurchasesResponse { Success = true, TotalCount = totalCount, Items = items };
        }
    }
}
