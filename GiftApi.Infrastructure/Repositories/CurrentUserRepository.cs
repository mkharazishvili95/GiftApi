using GiftApi.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GiftApi.Infrastructure.Repositories
{
    public class CurrentUserRepository : ICurrentUserRepository
    {
        readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? GetUser() => _httpContextAccessor.HttpContext?.User;

        public Guid? GetUserId()
        {
            var user = GetUser();
            if (user?.Identity is not { IsAuthenticated: true }) return null;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier)
                        ?? user.FindFirst("sub")
                        ?? user.FindFirst("userId");

            if (claim == null || !Guid.TryParse(claim.Value, out var userId)) return null;

            return userId;
        }
    }
}
