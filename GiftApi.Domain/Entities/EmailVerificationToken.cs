namespace GiftApi.Domain.Entities
{
    public class EmailVerificationToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public bool Used { get; set; }
    }
}