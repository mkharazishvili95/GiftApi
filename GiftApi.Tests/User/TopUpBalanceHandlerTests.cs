using GiftApi.Application.Features.Manage.User.Commands.TopUpBalance;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class TopUpBalanceHandlerTests
    {
        private ApplicationDbContext _db;
        private IUserRepository _userRepository;
        private TopUpBalanceHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _userRepository = new UserRepository(_db);
            _handler = new TopUpBalanceHandler(_userRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_TopUpBalance_Successfully()
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                Balance = 100,
                Type = UserType.User,
                RegisterDate = DateTime.UtcNow,
                IdentificationNumber = "1234567890",
                PhoneNumber = "+995599123456",      
                Password = "SecurePassword123!"   
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var command = new TopUpBalanceCommand
            {
                UserId = user.Id,
                Amount = 50
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(100, result.OldBalance);
            Assert.AreEqual(150, result.NewBalance);
            Assert.AreEqual("johndoe", result.UserName);

            var updatedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.AreEqual(150, updatedUser.Balance);
        }

        [Test]
        public async Task Should_Return_404_When_UserNotFound()
        {
            var command = new TopUpBalanceCommand
            {
                UserId = Guid.NewGuid(),
                Amount = 50
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.AreEqual("User not found", result.Message);
        }

        [Test]
        public async Task Should_Return_400_When_AmountIsZeroOrNegative()
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                UserName = "janedoe",
                Email = "jane@example.com",
                Balance = 200,
                Type = UserType.User,
                RegisterDate = DateTime.UtcNow,
                IdentificationNumber = "1234567890",
                PhoneNumber = "+995599123456",
                Password = "SecurePassword123!"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var commandZero = new TopUpBalanceCommand
            {
                UserId = user.Id,
                Amount = 0
            };
            var commandNegative = new TopUpBalanceCommand
            {
                UserId = user.Id,
                Amount = -50
            };

            var resultZero = await _handler.Handle(commandZero, CancellationToken.None);
            var resultNegative = await _handler.Handle(commandNegative, CancellationToken.None);

            Assert.IsFalse(resultZero.Success);
            Assert.AreEqual(400, resultZero.StatusCode);
            Assert.AreEqual("Amount must be greater than zero", resultZero.Message);

            Assert.IsFalse(resultNegative.Success);
            Assert.AreEqual(400, resultNegative.StatusCode);
            Assert.AreEqual("Amount must be greater than zero", resultNegative.Message);
        }

        [Test]
        public async Task Should_Fail_When_RepositoryFails()
        {
            var command = new TopUpBalanceCommand
            {
                UserId = Guid.NewGuid(),
                Amount = 50
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
        }
    }
}
