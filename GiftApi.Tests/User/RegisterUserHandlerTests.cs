using FluentAssertions;
using GiftApi.Application.User.Commands.Register;
using GiftApi.Application.User.Validators;
using GiftApi.Infrastructure.Data;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class RegisterUserHandlerTests
    {
        private ApplicationDbContext _db;
        private RegisterUserValidator _validator;
        private RegisterUserHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _validator = new RegisterUserValidator(_db);
            _handler = new RegisterUserHandler(_db, _validator);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Register_User_When_Command_Is_Valid()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "Misho",
                LastName = "Kharazishvili",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = Common.Enums.User.LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599111111",
                Email = "dummy@example.com",
                Gender = Common.Enums.User.Gender.Male
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.UserMessage.Should().Be("User registered successfully");

            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.UserName == command.PhoneNumber);
            userInDb.Should().NotBeNull();
            userInDb.FirstName.Should().Be(command.FirstName);
        }

        [Test]
        public async Task Should_Fail_When_PhoneNumber_Already_Exists()
        {
            var existingUser = new GiftApi.Core.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = "+995599111111",
                PhoneNumber = "+995599111111",
                Email = "existing@example.com",
                IdentificationNumber = "98765432109",
                Password = "hashed",
                FirstName = "Test",
                LastName = "User",
                RegisterDate = DateTime.UtcNow,
            };
            await _db.Users.AddAsync(existingUser);
            await _db.SaveChangesAsync();

            var command = new RegisterUserCommand
            {
                FirstName = "Misho",
                LastName = "Kharazishvili",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = Common.Enums.User.LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599111111"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.UserMessage.Should().Contain("already exists");
        }

        [Test]
        public async Task Should_Fail_When_Under18()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "Young",
                LastName = "User",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678902",
                DateOfBirth = DateTime.UtcNow.AddYears(-15),
                LoginType = Common.Enums.User.LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599222222"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.UserMessage.Should().Contain("at least 18 years old");
        }
    }
}
