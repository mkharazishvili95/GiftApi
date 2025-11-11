using FluentAssertions;
using GiftApi.Application.Features.User.Commands.Login;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class LoginUserHandlerTests
    {
        private ApplicationDbContext _db;
        private LoginUserValidator _validator;
        private IConfiguration _configuration;
        private LoginUserHandler _handler;
        private IUserRepository _userRepository;
        private ILogger<LoginUserHandler> _logger;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();

            _validator = new LoginUserValidator();

            var inMemorySettings = new Dictionary<string, string>
            {
                {"JwtSettings:Secret", "misho1995misho1995misho1995misho1995"},
                {"JwtSettings:Issuer", "misho1995"},
                {"JwtSettings:Audience", "misho1995"},
                {"JwtSettings:AccessTokenExpirationMinutes", "60"}
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _userRepository = new UserRepository(_db);

            _handler = new LoginUserHandler(_validator, _userRepository, _configuration, _logger);

        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedUserAsync(string userName, string password)
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Email = "test@example.com",
                PhoneNumber = "+995599111111",
                IdentificationNumber = "12345678901",
                FirstName = "Test",
                LastName = "User",
                RegisterDate = DateTime.UtcNow,
                Type = GiftApi.Domain.Enums.User.UserType.User
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Login_Successfully_When_Credentials_Are_Valid()
        {
            string password = "Secret123";
            await SeedUserAsync("misho", password);

            var command = new LoginUserCommand
            {
                UserName = "misho",
                Password = password
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Test]
        public async Task Should_Fail_When_UserName_Does_Not_Exist()
        {
            var command = new LoginUserCommand
            {
                UserName = "unknown",
                Password = "Secret123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
            result.Message.Should().Contain("Incorrect username or password");
        }

        [Test]
        public async Task Should_Fail_When_Password_Is_Incorrect()
        {
            await SeedUserAsync("misho", "CorrectPassword");

            var command = new LoginUserCommand
            {
                UserName = "misho",
                Password = "WrongPassword"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(401);
            result.Message.Should().Contain("Incorrect username or password");
        }

        [Test]
        public async Task Should_Fail_When_Validator_Fails()
        {
            var command = new LoginUserCommand
            {
                UserName = "",
                Password = ""
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("Username is required");
            result.Message.Should().Contain("Password is required");
        }

        [Test]
        public async Task Should_Save_LoginAudit_When_Login_Successful()
        {
            string password = "Secret123";
            await SeedUserAsync("misho", password);

            var command = new LoginUserCommand
            {
                UserName = "misho",
                Password = password
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeTrue();

            var audit = await _db.LoginAudits.FirstOrDefaultAsync(x => x.UserId == _db.Users.First().Id);
            audit.Should().NotBeNull();
            audit.UserId.Should().Be(_db.Users.First().Id);
            audit.LoginDate.Should().BeCloseTo(DateTime.UtcNow.AddHours(4), TimeSpan.FromSeconds(5));
        }
    }
}
