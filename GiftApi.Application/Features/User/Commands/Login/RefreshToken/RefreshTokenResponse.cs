namespace GiftApi.Application.Features.User.Commands.Login.RefreshToken
{
    public class RefreshTokenResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
