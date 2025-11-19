using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Auth.VerifyEmail;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;

namespace GiftApi.Tests.Auth
{
    [TestFixture]
    public class VerifyEmailHandlerTests
    {
        private GiftApi.Infrastructure.Data.ApplicationDbContext _db = null!;
        private UserRepository _userRepository = null!;
        private TestCurrentUserRepository _currentUser = null!;
        private VerifyEmailHandler _handler = null!;
        private Guid _userId;
        private string _rawToken = "";

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
                UserName = "verifyuser",
                Email = "verify@example.com",
                PhoneNumber = "+995599333444",
                IdentificationNumber = "12345678904",
                FirstName = "Verify",
                LastName = "User",
                Password = BCrypt.Net.BCrypt.HashPassword("Secret123"),
                RegisterDate = DateTime.UtcNow,
                Type = GiftApi.Domain.Enums.User.UserType.User,
                EmailVerified = false
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _rawToken = Guid.NewGuid().ToString("N");
            await _userRepository.CreateEmailVerificationTokenAsync(_userId, _rawToken, TimeSpan.FromHours(1));

            _handler = new VerifyEmailHandler(_userRepository, _currentUser);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task VerifyEmail_Should_Succeed_With_Valid_Token()
        {
            var cmd = new VerifyEmailCommand { Token = _rawToken };
            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.AlreadyVerified.Should().BeFalse();

            var user = await _userRepository.GetByIdAsync(_userId);
            user!.EmailVerified.Should().BeTrue();
            _db.EmailVerificationTokens.Single().Used.Should().BeTrue();
        }

        [Test]
        public async Task VerifyEmail_Should_Report_AlreadyVerified_On_Second_Attempt()
        {
            await _handler.Handle(new VerifyEmailCommand { Token = _rawToken }, CancellationToken.None);

            var secondTokenRaw = Guid.NewGuid().ToString("N");
            await _userRepository.CreateEmailVerificationTokenAsync(_userId, secondTokenRaw, TimeSpan.FromHours(1));

            var result = await _handler.Handle(new VerifyEmailCommand { Token = secondTokenRaw }, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.AlreadyVerified.Should().BeTrue();
            result.Message.Should().Contain("Already verified");
        }

        [Test]
        public async Task VerifyEmail_Should_Fail_When_Unauthorized()
        {
            _currentUser.SetUser(null);
            var result = await _handler.Handle(new VerifyEmailCommand { Token = _rawToken }, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
        }

        [Test]
        public async Task VerifyEmail_Should_Fail_When_Token_Invalid()
        {
            var cmd = new VerifyEmailCommand { Token = "invalidtoken" };
            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Invalid or expired token");
        }

        [Test]
        public async Task VerifyEmail_Should_Fail_When_Token_Missing()
        {
            var cmd = new VerifyEmailCommand { Token = "" };
            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Token required");
        }
    }
}