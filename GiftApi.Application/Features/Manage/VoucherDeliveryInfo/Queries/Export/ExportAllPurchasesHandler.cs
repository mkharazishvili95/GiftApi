using GiftApi.Application.Interfaces;
using MediatR;
using System.Globalization;
using System.Text;

namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Export
{
    public class ExportAllPurchasesHandler : IRequestHandler<ExportAllPurchasesQuery, ExportAllPurchasesResponse>
    {
        readonly IPurchaseRepository _purchaseRepository;

        public ExportAllPurchasesHandler(IPurchaseRepository purchaseRepository)
        {
            _purchaseRepository = purchaseRepository;
        }

        public async Task<ExportAllPurchasesResponse> Handle(ExportAllPurchasesQuery request, CancellationToken cancellationToken)
        {
            var purchases = await _purchaseRepository.GetAll() ?? new List<Domain.Entities.VoucherDeliveryInfo>();

            if (request.IsUsed.HasValue)
                purchases = purchases.Where(x => x.IsUsed == request.IsUsed.Value).ToList();

            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                var searchLower = request.SearchString.Trim().ToLower();
                purchases = purchases.Where(x =>
                    (!string.IsNullOrEmpty(x.RecipientName) && x.RecipientName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.RecipientEmail) && x.RecipientEmail.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(x.SenderName) && x.SenderName.ToLower().Contains(searchLower))
                ).ToList();
            }

            if (request.SenderId.HasValue)
                purchases = purchases.Where(x => x.SenderId == request.SenderId.Value).ToList();

            if (request.VoucherId.HasValue)
                purchases = purchases.Where(x => x.VoucherId == request.VoucherId.Value).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Id,VoucherId,VoucherTitle,SenderName,RecipientName,RecipientEmail,RecipientPhone,RecipientCity,RecipientAddress,SenderMessage,Quantity,IsUsed");
            foreach (var p in purchases)
            {
                string Esc(string? v) =>
                    v == null ? "" :
                    "\"" + v.Replace("\"", "\"\"") + "\"";

                sb.Append(Esc(p.Id.ToString())); sb.Append(',');
                sb.Append(Esc(p.VoucherId.ToString())); sb.Append(',');
                sb.Append(Esc(p.Voucher?.Title)); sb.Append(',');
                sb.Append(Esc(p.SenderName)); sb.Append(',');
                sb.Append(Esc(p.RecipientName)); sb.Append(',');
                sb.Append(Esc(p.RecipientEmail)); sb.Append(',');
                sb.Append(Esc(p.RecipientPhone)); sb.Append(',');
                sb.Append(Esc(p.RecipientCity)); sb.Append(',');
                sb.Append(Esc(p.RecipientAddress)); sb.Append(',');
                sb.Append(Esc(p.Message)); sb.Append(',');
                sb.Append(Esc(p.Quantity?.ToString(CultureInfo.InvariantCulture))); sb.Append(',');
                var isUsedStr = p.IsUsed.HasValue ? (p.IsUsed.Value ? "true" : "false") : "";
                sb.Append(Esc(isUsedStr));
                sb.AppendLine();
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return new ExportAllPurchasesResponse
            {
                Data = bytes,
                FileName = $"purchases-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
                ContentType = "text/csv"
            };
        }
    }
}
