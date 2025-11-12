using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Password.Change;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class ChangePasswordHandlerTests
    {
        private ApplicationDbContext _db;
        private IUserRepository _userRepository;
        private ICurrentUserRepository _currentUserRepository;
        private ChangePasswordHandler _handler;
        private ChangePasswordValidation _validator;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _userRepository = new UserRepository(_db);
            _currentUserRepository = new TestCurrentUserRepository();
            _validator = new ChangePasswordValidation();
            _handler = new ChangePasswordHandler(_userRepository, _currentUserRepository, _validator);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task<GiftApi.Domain.Entities.User> SeedUserAsync(string plainPassword)
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                UserName = "testuser",
                Email = "test@example.com",
                PhoneNumber = "+995599111111",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Password = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                RegisterDate = DateTime.UtcNow,
                Balance = 0,
                Type = Domain.Enums.User.UserType.User
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
            return user;
        }

        [Test]
        public async Task Should_ChangePassword_Successfully()
        {
            var oldPwd = "OldSecret123";
            var newPwd = "NewSecret456";
            var user = await SeedUserAsync(oldPwd);
            var userId = user.Id;

            _db.Entry(user).State = EntityState.Detached;

            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(userId);

            var command = new ChangePasswordCommand
            {
                OldPassword = oldPwd,
                NewPassword = newPwd,
                ConfirmPassword = newPwd
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("Password changed");

            var updated = await _db.Users.AsNoTracking().FirstAsync(x => x.Id == userId);
            BCrypt.Net.BCrypt.Verify(newPwd, updated.Password).Should().BeTrue();
            BCrypt.Net.BCrypt.Verify(oldPwd, updated.Password).Should().BeFalse();
        }

        [Test]
        public async Task Should_Fail_When_Unauthorized()
        {
            var command = new ChangePasswordCommand
            {
                OldPassword = "any",
                NewPassword = "newpass123",
                ConfirmPassword = "newpass123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
            result.Message.Should().Contain("Unauthorized");
        }

        [Test]
        public async Task Should_Fail_When_User_Not_Found()
        {
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(Guid.NewGuid());

            var command = new ChangePasswordCommand
            {
                OldPassword = "oldpwd",
                NewPassword = "newpwd123",
                ConfirmPassword = "newpwd123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Contain("User not found");
        }

        [Test]
        public async Task Should_Fail_When_OldPassword_Incorrect()
        {
            var user = await SeedUserAsync("CorrectOld123");
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var command = new ChangePasswordCommand
            {
                OldPassword = "WrongOld999",
                NewPassword = "NewPwd123",
                ConfirmPassword = "NewPwd123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Old password is incorrect");
        }

        [Test]
        public async Task Should_Fail_Validation_When_NewPassword_TooShort()
        {
            var user = await SeedUserAsync("OldPwd123");
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var command = new ChangePasswordCommand
            {
                OldPassword = "OldPwd123",
                NewPassword = "123",
                ConfirmPassword = "123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("at least 6 characters");
        }

        [Test]
        public async Task Should_Fail_Validation_When_NewPassword_Equals_Old()
        {
            var user = await SeedUserAsync("SamePwd123");
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var command = new ChangePasswordCommand
            {
                OldPassword = "SamePwd123",
                NewPassword = "SamePwd123",
                ConfirmPassword = "SamePwd123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("different from old password");
        }

        [Test]
        public async Task Should_Fail_Validation_When_Confirm_Not_Matching()
        {
            var user = await SeedUserAsync("OldPwd123");
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var command = new ChangePasswordCommand
            {
                OldPassword = "OldPwd123",
                NewPassword = "NewPwd123",
                ConfirmPassword = "Mismatch999"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Passwords do not match");
        }

        [Test]
        public async Task Should_Fail_Validation_When_Required_Fields_Missing()
        {
            var user = await SeedUserAsync("OldPwd123");
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var command = new ChangePasswordCommand
            {
                OldPassword = "",
                NewPassword = "",
                ConfirmPassword = ""
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Old password is required");
            result.Message.Should().Contain("New password is required");
            result.Message.Should().Contain("Confirm password is required");
        }

        private class TestCurrentUserRepository : ICurrentUserRepository
        {
            private Guid? _userId;

            public void SetUserId(Guid userId) => _userId = userId;
            public Guid? GetUserId() => _userId;
            public ClaimsPrincipal? GetUser() => _userId.HasValue
                ? new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _userId.ToString()) }))
                : null;
        }
    }
}