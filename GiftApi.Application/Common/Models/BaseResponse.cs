namespace GiftApi.Application.Common.Models
{
    public class BaseResponse
    {
        public bool? Success { get; set; }
        public string? Message { get; set; }
        public int? StatusCode { get; set; }
    }
}
