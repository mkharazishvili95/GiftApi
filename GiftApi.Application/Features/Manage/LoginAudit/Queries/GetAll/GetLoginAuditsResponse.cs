using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.LoginAudit.Queries.GetAll
{
    public class GetLoginAuditsResponse : BaseResponse
    {
        public int TotalCount { get; set; }
        public List<LoginAuditItem> Items { get; set; } = new();
    }

    public class LoginAuditItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime LoginDate { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
    }
}