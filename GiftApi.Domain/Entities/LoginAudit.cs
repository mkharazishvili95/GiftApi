namespace GiftApi.Domain.Entities
{
    public class LoginAudit
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoginDate { get; set; }
    }
}
