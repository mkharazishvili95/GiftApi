using GiftApi.Application.Common.Models;
using MediatR;

namespace GiftApi.Application.Features.Manage.LoginAudit.Queries.GetAll
{
    public class GetLoginAuditsQuery : IRequest<GetLoginAuditsResponse>
    {
        public Guid? UserId { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public PaginationModel Pagination { get; set; } = new();
    }
}