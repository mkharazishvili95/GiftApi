using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Auth.ResetPassword;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;

namespace GiftApi.Tests.Auth
{
    [TestFixture]
    public class ResetPasswordHandlerTests
    {
        private GiftApi.Infrastructure.Data.ApplicationDbContext _db = null!;
        private UserRepository _userRepository = null!;
        private ResetPasswordHandler _handler = null!;
        private Guid _userId;
        private string _rawToken = "";

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _handler = new ResetPasswordHandler(_userRepository);

            _userId = Guid.NewGuid();
            var user = new GiftApi.Domain.Entities.User
            {
                Id = _userId,
                UserName = "resetuser",
                Email = "reset@example.com",
                PhoneNumber = "+995599111222",
                IdentificationNumber = "12345678903",
                FirstName = "Reset",
                LastName = "User",
                Password = BCrypt.Net.BCrypt.HashPassword("OldPass123"),
                RegisterDate = DateTime.UtcNow,
                Type = GiftApi.Domain.Enums.User.UserType.User
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _rawToken = Guid.NewGuid().ToString("N");
            await _userRepository.CreatePasswordResetTokenAsync(_userId, _rawToken, TimeSpan.FromMinutes(30));
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task ResetPassword_Should_Succeed_With_Valid_Token()
        {
            var cmd = new ResetPasswordCommand
            {
                Token = _rawToken,
                NewPassword = "NewStrongPass123",
                ConfirmPassword = "NewStrongPass123"
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);

            var user = await _userRepository.GetByIdAsync(_userId);
            BCrypt.Net.BCrypt.Verify("NewStrongPass123", user!.Password).Should().BeTrue();

            _db.PasswordResetTokens.Single().Used.Should().BeTrue();
        }

        [Test]
        public async Task ResetPassword_Should_Fail_When_Token_Invalid()
        {
            var cmd = new ResetPasswordCommand
            {
                Token = "invalid",
                NewPassword = "Pass12345",
                ConfirmPassword = "Pass12345"
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Invalid or expired token");
        }

        [Test]
        public async Task ResetPassword_Should_Fail_When_Passwords_Do_Not_Match()
        {
            var cmd = new ResetPasswordCommand
            {
                Token = _rawToken,
                NewPassword = "PassA123",
                ConfirmPassword = "PassB123"
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Passwords do not match");
        }

        [Test]
        public async Task ResetPassword_Should_Fail_When_NewPassword_Missing()
        {
            var cmd = new ResetPasswordCommand
            {
                Token = _rawToken,
                NewPassword = "",
                ConfirmPassword = ""
            };

            var result = await _handler.Handle(cmd, CancellationToken.None);
            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("New password required");
        }
    }
}