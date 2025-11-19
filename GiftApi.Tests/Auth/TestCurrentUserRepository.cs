using System.Security.Claims;
using GiftApi.Application.Interfaces;

namespace GiftApi.Tests.Auth
{
    public class TestCurrentUserRepository : ICurrentUserRepository
    {
        private Guid? _userId;
        public TestCurrentUserRepository(Guid? userId) => _userId = userId;

        public Guid? GetUserId() => _userId;

        public ClaimsPrincipal? GetUser()
        {
            if (_userId == null) return null;
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userId.Value.ToString())
            }, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        public void SetUser(Guid? userId) => _userId = userId;
    }
}