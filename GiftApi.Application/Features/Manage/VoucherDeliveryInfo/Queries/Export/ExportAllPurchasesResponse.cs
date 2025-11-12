namespace GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Export
{
    public class ExportAllPurchasesResponse
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/csv";
    }
}