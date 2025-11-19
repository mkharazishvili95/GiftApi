using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Auth.Logout;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;

namespace GiftApi.Tests.Auth
{
    [TestFixture]
    public class LogoutHandlerTests
    {
        private GiftApi.Infrastructure.Data.ApplicationDbContext _db = null!;
        private UserRepository _userRepository = null!;
        private TestCurrentUserRepository _currentUser = null!;
        private LogoutHandler _handler = null!;
        private Guid _userId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _userId = Guid.NewGuid();
            _currentUser = new TestCurrentUserRepository(_userId);

            var user = new GiftApi.Domain.Entities.User
            {
                Id = _userId,
                UserName = "tester",
                Email = "tester@example.com",
                PhoneNumber = "+995599000000",
                IdentificationNumber = "12345678901",
                FirstName = "Test",
                LastName = "User",
                Password = BCrypt.Net.BCrypt.HashPassword("Secret123"),
                RegisterDate = DateTime.UtcNow,
                RefreshToken = "some-refresh",
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(1),
                Type = GiftApi.Domain.Enums.User.UserType.User
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _handler = new LogoutHandler(_userRepository, _currentUser);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Logout_Should_Succeed_And_Revoke_Token()
        {
            var result = await _handler.Handle(new LogoutCommand(), CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            var updated = await _userRepository.GetByIdAsync(_userId);
            updated!.RefreshToken.Should().BeNull();
            updated.RefreshTokenExpiry.Should().BeNull();
        }

        [Test]
        public async Task Logout_Should_Fail_When_Unauthorized()
        {
            _currentUser.SetUser(null);
            var result = await _handler.Handle(new LogoutCommand(), CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
        }

        [Test]
        public async Task Logout_Should_Fail_When_User_Not_Found()
        {
            var tracked = await _db.Users.FindAsync(_userId);
            _db.Users.Remove(tracked!);
            await _db.SaveChangesAsync();

            var result = await _handler.Handle(new LogoutCommand(), CancellationToken.None);
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
        }
    }
}