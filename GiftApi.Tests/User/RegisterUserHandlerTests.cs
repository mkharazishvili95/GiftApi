using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Register;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Tests.Helpers;
using GiftApi.Tests.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class RegisterUserHandlerTests
    {
        private ApplicationDbContext _db;
        private IUserRepository _repository;
        private RegisterUserValidator _validator;
        private RegisterUserHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repository = new InMemoryUserRepository(_db);
            _validator = new RegisterUserValidator(_repository);
            _handler = new RegisterUserHandler(_repository, _validator);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Register_User_With_PhoneNumber_LoginType()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "Misho",
                LastName = "Kharazishvili",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599111111",
                Email = "dummy@example.com",
                Gender = Gender.Male
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Message.Should().Be("User registered successfully");

            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.UserName == command.PhoneNumber);
            userInDb.Should().NotBeNull();
            userInDb.FirstName.Should().Be(command.FirstName);
        }

        [Test]
        public async Task Should_Register_User_With_Email_LoginType()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "Anna",
                LastName = "Smith",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678902",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                LoginType = LoginIdentifierType.Email,
                Email = "anna@example.com",
                PhoneNumber = "+995599222222",
                Gender = Gender.Female
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Message.Should().Be("User registered successfully");

            var userInDb = await _db.Users.FirstOrDefaultAsync(u => u.UserName == command.Email);
            userInDb.Should().NotBeNull();
            userInDb.FirstName.Should().Be(command.FirstName);
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
                IdentificationNumber = "12345678903",
                DateOfBirth = DateTime.UtcNow.AddYears(-15),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599333333"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("at least 18 years old");
        }

        [Test]
        public async Task Should_Fail_When_Passwords_Do_Not_Match()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "Mismatch",
                LastName = "User",
                Password = "Secret123",
                ConfirmPassword = "Secret456",
                IdentificationNumber = "12345678904",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599444444"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Passwords do not match");
        }

        [Test]
        public async Task Should_Fail_When_FirstName_Is_Empty()
        {
            var command = new RegisterUserCommand
            {
                FirstName = "",
                LastName = "User",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678905",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599555555"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("First name is required");
        }

        [Test]
        public async Task Should_Fail_When_PhoneNumber_Already_Exists()
        {
            await _db.Users.AddAsync(new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = "+995599666666",
                PhoneNumber = "+995599666666",
                Email = "existing@example.com",
                IdentificationNumber = "98765432109",
                Password = "hashed",
                FirstName = "Test",
                LastName = "User",
                RegisterDate = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();

            var command = new RegisterUserCommand
            {
                FirstName = "Misho",
                LastName = "Kharazishvili",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678906",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599666666"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("already exists");
        }

        [Test]
        public async Task Should_Fail_When_Email_Already_Exists()
        {
            await _db.Users.AddAsync(new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = "existing@example.com",
                Email = "existing@example.com",
                PhoneNumber = "+995599777777",
                IdentificationNumber = "98765432108",
                Password = "hashed",
                FirstName = "Test",
                LastName = "User",
                RegisterDate = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();

            var command = new RegisterUserCommand
            {
                FirstName = "Anna",
                LastName = "Smith",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678907",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                LoginType = LoginIdentifierType.Email,
                Email = "existing@example.com",
                PhoneNumber = "+995599888888"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("already exists");
        }

        [Test]
        public async Task Should_Fail_When_IdentificationNumber_Already_Exists()
        {
            await _db.Users.AddAsync(new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = "uniqueuser",
                Email = "unique@example.com",
                PhoneNumber = "+995599999999",
                IdentificationNumber = "12345678908",
                Password = "hashed",
                FirstName = "Test",
                LastName = "User",
                RegisterDate = DateTime.UtcNow,
            });
            await _db.SaveChangesAsync();

            var command = new RegisterUserCommand
            {
                FirstName = "New",
                LastName = "User",
                Password = "Secret123",
                ConfirmPassword = "Secret123",
                IdentificationNumber = "12345678908",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                LoginType = LoginIdentifierType.PhoneNumber,
                PhoneNumber = "+995599101010"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("already exists");
        }
    }
}