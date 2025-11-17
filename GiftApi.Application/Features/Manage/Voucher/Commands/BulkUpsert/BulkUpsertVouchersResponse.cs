namespace GiftApi.Application.Features.Manage.Voucher.Commands.BulkUpsert
{
    public class BulkUpsertVouchersResponse
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<BulkVoucherItemResult> Results { get; set; } = new();
        public int CreatedCount => Results.Count(r => r.Created && r.Success);
        public int UpdatedCount => Results.Count(r => r.Updated && r.Success);
        public int FailedCount => Results.Count(r => !r.Success);
    }

    public class BulkVoucherItemResult
    {
        public Guid? InputId { get; set; }        
        public Guid? FinalId { get; set; }     
        public bool Created { get; set; }
        public bool Updated { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}