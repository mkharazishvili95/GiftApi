using FluentAssertions;
using GiftApi.Application.Features.Manage.Queries.GetUser;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class GetUserHandlerTests
    {
        private ApplicationDbContext _db;
        private IUserRepository _userRepository;
        private GetUserHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _handler = new GetUserHandler(_userRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task<GiftApi.Domain.Entities.User> SeedUserAsync()
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Misho",
                LastName = "Kharazishvili",
                UserName = "misho",
                Password = BCrypt.Net.BCrypt.HashPassword("password123"),
                Email = "misho@example.com",
                PhoneNumber = "+995599111111",
                IdentificationNumber = "12345678901",
                DateOfBirth = new DateTime(2000, 5, 15),
                Gender = Domain.Enums.User.Gender.Male,
                RegisterDate = DateTime.UtcNow,
                Balance = 100,
                Type = Domain.Enums.User.UserType.User
            };

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return user;
        }

        [Test]
        public async Task Should_Return_User_When_Exists()
        {
            var user = await SeedUserAsync();

            var query = new GetUserQuery { Id = user.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.Id.Should().Be(user.Id);
            result.FirstName.Should().Be(user.FirstName);
            result.LastName.Should().Be(user.LastName);
            result.Email.Should().Be(user.Email);
            result.PhoneNumber.Should().Be(user.PhoneNumber);
            result.Balance.Should().Be(user.Balance);
        }

        [Test]
        public async Task Should_Return_400_When_Id_Is_Empty()
        {
            var query = new GetUserQuery { Id = Guid.Empty };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(400);
            result.Message.Should().Contain("UserId is required");
        }

        [Test]
        public async Task Should_Return_404_When_User_Not_Found()
        {
            var query = new GetUserQuery { Id = Guid.NewGuid() };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(404);
            result.Message.Should().Contain("User not found");
        }
    }
}
