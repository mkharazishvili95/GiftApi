namespace GiftApi.Application.Common.Responses
{
    public class BaseResponse
    {
        public bool? Success { get; set; } 
        public string? UserMessage { get; set; }
        public int? StatusCode { get; set; } 
    }
}
