using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Auth.ForgotPassword;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;

namespace GiftApi.Tests.Auth
{
    [TestFixture]
    public class ForgotPasswordHandlerTests
    {
        private GiftApi.Infrastructure.Data.ApplicationDbContext _db = null!;
        private UserRepository _userRepository = null!;
        private ForgotPasswordHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _handler = new ForgotPasswordHandler(_userRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedUserAsync(string email)
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = "testuser",
                Email = email,
                PhoneNumber = "+995599999999",
                IdentificationNumber = "12345678902",
                FirstName = "Test",
                LastName = "User",
                Password = BCrypt.Net.BCrypt.HashPassword("Secret123"),
                RegisterDate = DateTime.UtcNow,
                Type = GiftApi.Domain.Enums.User.UserType.User
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task ForgotPassword_Should_Return_Token_For_Existing_User()
        {
            await SeedUserAsync("user@example.com");

            var result = await _handler.Handle(new ForgotPasswordCommand { Email = "user@example.com" }, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.PublicToken.Should().NotBeNullOrEmpty();
            _db.PasswordResetTokens.Count().Should().Be(1);
        }

        [Test]
        public async Task ForgotPassword_Should_Return_Generic_Success_For_NonExisting_User()
        {
            var result = await _handler.Handle(new ForgotPasswordCommand { Email = "absent@example.com" }, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.PublicToken.Should().BeNull();
            _db.PasswordResetTokens.Count().Should().Be(0);
        }

        [Test]
        public async Task ForgotPassword_Should_Fail_When_Email_Missing()
        {
            var result = await _handler.Handle(new ForgotPasswordCommand { Email = "" }, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Email required");
        }
    }
}