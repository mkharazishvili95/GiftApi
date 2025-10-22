using FluentAssertions;
using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Manage.Queries.GetAllUsers;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.User
{
    [TestFixture]
    public class GetAllUsersHandlerTests
    {
        private ApplicationDbContext _db;
        private IUserRepository _userRepository;
        private GetAllUsersHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _handler = new GetAllUsersHandler(_userRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedUsersAsync(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var user = new GiftApi.Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = $"First{i}",
                    LastName = $"Last{i}",
                    UserName = $"User{i}",
                    Email = $"user{i}@example.com",
                    Password = "Password123",
                    PhoneNumber = $"+995599{i:D6}",
                    IdentificationNumber = $"{12345678900 + i}",
                    Type = UserType.User,
                    RegisterDate = DateTime.UtcNow.AddDays(-i),
                    Balance = i * 100
                };
                await _db.Users.AddAsync(user);
            }
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_AllUsers_WithPagination()
        {
            await SeedUsersAsync(10);

            var query = new GetAllUsersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.TotalCount.Should().Be(10);
            result.Items.Count.Should().Be(5);
            result.Items.First().RegisterDate.Should().BeBefore(result.Items.Last().RegisterDate);
        }

        [Test]
        public async Task Should_Return_EmptyList_WhenNoUsers()
        {
            var query = new GetAllUsersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
        }

        [Test]
        public async Task Should_Return_CorrectPagination_SecondPage()
        {
            await SeedUsersAsync(12);

            var query = new GetAllUsersQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.StatusCode.Should().Be(200);
            result.TotalCount.Should().Be(12);
            result.Items.Count.Should().Be(5);
            result.Items.First().UserName.Should().Be("User6");
        }
    }
}
