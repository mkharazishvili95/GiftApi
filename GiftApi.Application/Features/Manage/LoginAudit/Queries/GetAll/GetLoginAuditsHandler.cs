using GiftApi.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Application.Features.Manage.LoginAudit.Queries.GetAll
{
    public class GetLoginAuditsHandler : IRequestHandler<GetLoginAuditsQuery, GetLoginAuditsResponse>
    {
        readonly IUserRepository _userRepository;
        readonly ICurrentUserRepository _currentUser;

        public GetLoginAuditsHandler(IUserRepository userRepository, ICurrentUserRepository currentUser)
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
        }

        public async Task<GetLoginAuditsResponse> Handle(GetLoginAuditsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _userRepository.GetLoginAuditsAsync(
                request.UserId,
                request.FromUtc,
                request.ToUtc,
                request.Pagination.Page,
                request.Pagination.PageSize,
                cancellationToken);

            if (total == 0)
                return new GetLoginAuditsResponse { Success = true, TotalCount = 0, Items = new(), Message = "No login audits found" };

            var userIds = items.Select(x => x.UserId).Distinct().ToList();
            var usersMap = await _userRepository.GetAllUsers()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.UserName })
                .ToDictionaryAsync(x => x.Id, x => x, cancellationToken);

            var responseItems = items.Select(a => new LoginAuditItem
            {
                Id = a.Id,
                UserId = a.UserId,
                LoginDate = a.LoginDate,
                UserEmail = usersMap.TryGetValue(a.UserId, out var u) ? u.Email : null,
                UserName = usersMap.TryGetValue(a.UserId, out u) ? u.UserName : null
            }).ToList();

            return new GetLoginAuditsResponse
            {
                StatusCode = 200,
                Success = true,
                TotalCount = total,
                Items = responseItems
            };
        }
    }
}