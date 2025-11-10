using GiftApi.Application.Features.User.Queries.GetCurrent;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class GetCurrentUserHandlerTests
    {
        private ApplicationDbContext _dbContext;
        private IUserRepository _userRepository;
        private ICurrentUserRepository _currentUserRepository;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _userRepository = new UserRepository(_dbContext);

            _currentUserRepository = new TestCurrentUserRepository();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task Handle_ReturnsCurrentUser_WhenUserExists()
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john@example.com",
                Balance = 100,
                Type = Domain.Enums.User.UserType.User,
                IdentificationNumber = "1234567890",
                Password = "hashedpassword",
                PhoneNumber = "+995555123456"
            };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(user.Id);

            var handler = new GetCurrentUserHandler(_userRepository, _currentUserRepository);
            var query = new GetCurrentUserQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(user.Id, result.Id);
            Assert.AreEqual(user.UserName, result.UserName);
        }

        [Test]
        public async Task Handle_ReturnsUnauthorized_WhenUserIdIsNull()
        {
            var handler = new GetCurrentUserHandler(_userRepository, _currentUserRepository);
            var query = new GetCurrentUserQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(401, result.StatusCode);
        }

        [Test]
        public async Task Handle_ReturnsNotFound_WhenUserDoesNotExist()
        {
            ((TestCurrentUserRepository)_currentUserRepository).SetUserId(Guid.NewGuid());

            var handler = new GetCurrentUserHandler(_userRepository, _currentUserRepository);
            var query = new GetCurrentUserQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
        }

        private class TestCurrentUserRepository : ICurrentUserRepository
        {
            private Guid? _userId;

            public void SetUserId(Guid userId) => _userId = userId;

            public Guid? GetUserId() => _userId;

            public ClaimsPrincipal? GetUser()
            {
                throw new NotImplementedException();
            }
        }
    }
}
