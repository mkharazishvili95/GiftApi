using FluentAssertions;
using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Manage.LoginAudit.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Domain.Enums.User;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;
using System.Security.Claims;

namespace GiftApi.Tests.LoginAudit
{
    [TestFixture]
    public class GetLoginAuditsHandlerTests
    {
        ApplicationDbContext _db;
        IUserRepository _userRepository;
        ICurrentUserRepository _currentUserRepository;
        GetLoginAuditsHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _userRepository = new UserRepository(_db);
            _currentUserRepository = new TestCurrentUserRepository();
            _handler = new GetLoginAuditsHandler(_userRepository, _currentUserRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        class TestCurrentUserRepository : ICurrentUserRepository
        {
            public ClaimsPrincipal? GetUser() => null;
            public Guid? GetUserId() => null;
        }

        async Task<Guid> SeedUserAsync(string email)
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "First",
                LastName = "Last",
                IdentificationNumber = Guid.NewGuid().ToString("N").Substring(0,11),
                PhoneNumber = $"+9955{Random.Shared.Next(1000000, 9999999)}",
                Email = email,
                UserName = email.Split('@')[0],
                Password = "pwd",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Balance = 0,
                Type = UserType.User,
                RegisterDate = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.Id;
        }

        async Task SeedAuditsAsync(Guid userId, int count, DateTime start)
        {
            for (int i = 0; i < count; i++)
            {
                _db.LoginAudits.Add(new GiftApi.Domain.Entities.LoginAudit
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LoginDate = start.AddMinutes(i)
                });
            }
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Empty_When_No_Audits()
        {
            var query = new GetLoginAuditsQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();
            result.Message.Should().Be("No login audits found");
        }

        [Test]
        public async Task Should_Return_Audits_With_Pagination()
        {
            var userId = await SeedUserAsync("user@example.com");
            await SeedAuditsAsync(userId, 12, DateTime.UtcNow.AddHours(-1));

            var query = new GetLoginAuditsQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.TotalCount.Should().Be(12);
            result.Items.Count.Should().Be(5);
            result.Items.Should().BeInDescendingOrder(x => x.LoginDate);
            result.Items.All(x => x.UserEmail == "user@example.com").Should().BeTrue();
        }

        [Test]
        public async Task Should_Filter_By_UserId()
        {
            var user1 = await SeedUserAsync("first@example.com");
            var user2 = await SeedUserAsync("second@example.com");

            await SeedAuditsAsync(user1, 5, DateTime.UtcNow.AddHours(-2));
            await SeedAuditsAsync(user2, 3, DateTime.UtcNow.AddHours(-1));

            var query = new GetLoginAuditsQuery
            {
                UserId = user2,
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(3);
            result.Items.Select(x => x.UserId).Distinct().Single().Should().Be(user2);
            result.Items.All(x => x.UserEmail == "second@example.com").Should().BeTrue();
        }

        [Test]
        public async Task Should_Filter_By_DateRange()
        {
            var userId = await SeedUserAsync("date@example.com");
            var baseTime = DateTime.UtcNow.AddHours(-4);

            await SeedAuditsAsync(userId, 10, baseTime);

            var fromUtc = baseTime.AddMinutes(3);
            var toUtc = baseTime.AddMinutes(7);

            var query = new GetLoginAuditsQuery
            {
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Pagination = new PaginationModel { Page = 1, PageSize = 20 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(5);
            result.Items.All(x => x.LoginDate >= fromUtc && x.LoginDate <= toUtc).Should().BeTrue();
        }

        [Test]
        public async Task Should_Return_Correct_SecondPage()
        {
            var userId = await SeedUserAsync("paginate@example.com");
            await SeedAuditsAsync(userId, 9, DateTime.UtcNow.AddHours(-1));

            var query = new GetLoginAuditsQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(9);
            result.Items.Count.Should().Be(4);
        }

        [Test]
        public async Task Should_Normalize_Invalid_Page_And_PageSize()
        {
            var userId = await SeedUserAsync("normalize@example.com");
            await SeedAuditsAsync(userId, 15, DateTime.UtcNow.AddHours(-1));

            var query = new GetLoginAuditsQuery
            {
                Pagination = new PaginationModel { Page = 0, PageSize = 0 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.TotalCount.Should().Be(15);
            result.Items.Count.Should().Be(10);
        }

        [Test]
        public async Task Should_Map_User_Email_And_UserName()
        {
            var userId = await SeedUserAsync("mapped@example.com");
            await SeedAuditsAsync(userId, 2, DateTime.UtcNow.AddMinutes(-10));

            var query = new GetLoginAuditsQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 5 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Count.Should().Be(2);
            result.Items.All(x => x.UserEmail == "mapped@example.com").Should().BeTrue();
            result.Items.All(x => x.UserName == "mapped").Should().BeTrue();
        }
    }
}