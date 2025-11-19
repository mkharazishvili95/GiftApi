using FluentAssertions;
using GiftApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class CurrentUserRepositoryTests
    {
        IHttpContextAccessor _accessor = null!;
        CurrentUserRepository _repo = null!;

        [SetUp]
        public void SetUp()
        {
            var context = new DefaultHttpContext();
            _accessor = new HttpContextAccessor { HttpContext = context };
            _repo = new CurrentUserRepository(_accessor);
        }

        [Test]
        public void GetUserId_Should_Return_Null_When_No_User()
        {
            _repo.GetUserId().Should().BeNull();
        }

        [Test]
        public void GetUserId_Should_Return_Null_When_Unauthenticated()
        {
            var identity = new ClaimsIdentity();
            _accessor.HttpContext!.User = new ClaimsPrincipal(identity);
            _repo.GetUserId().Should().BeNull();
        }

        [Test]
        public void GetUserId_Should_Read_NameIdentifier()
        {
            var userId = Guid.NewGuid();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "TestAuth");
            _accessor.HttpContext!.User = new ClaimsPrincipal(identity);
            _repo.GetUserId().Should().Be(userId);
        }

        [Test]
        public void GetUserId_Should_Fallback_To_Sub()
        {
            var userId = Guid.NewGuid();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", userId.ToString())
            }, "TestAuth");
            _accessor.HttpContext!.User = new ClaimsPrincipal(identity);
            _repo.GetUserId().Should().Be(userId);
        }

        [Test]
        public void GetUserId_Should_Fallback_To_UserId()
        {
            var userId = Guid.NewGuid();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("userId", userId.ToString())
            }, "TestAuth");
            _accessor.HttpContext!.User = new ClaimsPrincipal(identity);
            _repo.GetUserId().Should().Be(userId);
        }

        [Test]
        public void GetUser_Should_Return_Principal()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "tester") }, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _accessor.HttpContext!.User = principal;
            _repo.GetUser().Should().BeSameAs(principal);
        }
    }
}