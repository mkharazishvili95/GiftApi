using GiftApi.Application.Features.Manage.Dashboard.Queries.GetGlobalMetrics;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.Dashboard
{
    [TestFixture]
    public class GetGlobalMetricsHandlerTests
    {
        ApplicationDbContext _db;
        IDashboardRepository _dashboardRepository;
        GetGlobalMetricsHandler _handler;

        [SetUp]
        public void Setup()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _dashboardRepository = new DashboardRepository(_db);
            _handler = new GetGlobalMetricsHandler(_dashboardRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        async Task<Guid> SeedUserAsync(string email, bool active = true)
        {
            var user = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "F",
                LastName = "L",
                IdentificationNumber = Guid.NewGuid().ToString("N").Substring(0, 11),
                PhoneNumber = $"+9955{Random.Shared.Next(1000000, 9999999)}",
                Email = email,
                UserName = email.Split('@')[0],
                Password = "pwd",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Balance = 0,
                Type = Domain.Enums.User.UserType.User,
                RegisterDate = DateTime.UtcNow
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.Id;
        }

        async Task<Guid> SeedVoucherAsync(string title, decimal amount, bool isActive = true, bool isDeleted = false)
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "",
                Amount = amount,
                IsPercentage = false,
                BrandId = null,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                SoldCount = 0,
                IsActive = isActive,
                IsDeleted = isDeleted,
                CreateDate = DateTime.UtcNow.AddDays(-2)
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();
            return voucher.Id;
        }

        async Task SeedPurchaseAsync(Guid voucherId, int quantity, DateTime createDateLocal, bool isUsed = false)
        {
            var info = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucherId,
                RecipientName = "R",
                RecipientPhone = "000",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                IsUsed = isUsed,
                Quantity = quantity,
                CreateDate = createDateLocal
            };
            _db.VoucherDeliveryInfos.Add(info);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Zero_When_No_Data()
        {
            var result = await _handler.Handle(new GetGlobalMetricsQuery(), CancellationToken.None);

            Assert.That(result.UsersCount, Is.EqualTo(0));
            Assert.That(result.ActiveVouchers, Is.EqualTo(0));
            Assert.That(result.SoldToday, Is.EqualTo(0));
            Assert.That(result.RevenueToday, Is.EqualTo(0m));
        }

        [Test]
        public async Task Should_Calculate_Today_Sales_And_Revenue()
        {
            await SeedUserAsync("u1@example.com");
            await SeedUserAsync("u2@example.com");

            var v1 = await SeedVoucherAsync("VoucherA", 10m, isActive: true);
            var v2 = await SeedVoucherAsync("VoucherB", 25.5m, isActive: true);
            var v3 = await SeedVoucherAsync("VoucherC", 7m, isActive: false); 

            var utcNow = DateTime.UtcNow;
            var todayStartLocal = utcNow.Date.AddHours(4);
            var withinToday1 = todayStartLocal.AddHours(1);
            var withinToday2 = todayStartLocal.AddHours(5); 
            var yesterdayLocal = todayStartLocal.AddDays(-1).AddHours(10); 

            await SeedPurchaseAsync(v1, quantity: 2, createDateLocal: withinToday1);
            await SeedPurchaseAsync(v2, quantity: 3, createDateLocal: withinToday2);

            await SeedPurchaseAsync(v1, quantity: 5, createDateLocal: yesterdayLocal);

            var result = await _handler.Handle(new GetGlobalMetricsQuery(), CancellationToken.None);

            Assert.That(result.UsersCount, Is.EqualTo(2));

            Assert.That(result.ActiveVouchers, Is.EqualTo(2));

            Assert.That(result.SoldToday, Is.EqualTo(2));

            Assert.That(result.RevenueToday, Is.EqualTo(96.5m));
        }

        [Test]
        public async Task Should_Exclude_Purchases_Before_TodayStart()
        {
            var v1 = await SeedVoucherAsync("VoucherA", 5m, isActive: true);

            var utcNow = DateTime.UtcNow;
            var todayStartLocal = utcNow.Date.AddHours(4);
            var boundaryMinus = todayStartLocal.AddMinutes(-1); 
            var boundaryPlus = todayStartLocal.AddMinutes(1);  

            await SeedPurchaseAsync(v1, 1, boundaryMinus); 
            await SeedPurchaseAsync(v1, 1, boundaryPlus);  

            var result = await _handler.Handle(new GetGlobalMetricsQuery(), CancellationToken.None);

            Assert.That(result.SoldToday, Is.EqualTo(1));
            Assert.That(result.RevenueToday, Is.EqualTo(5m));
        }

        [Test]
        public async Task Should_Not_Count_Deleted_Or_Inactive_Vouchers_As_Active()
        {
            var active = await SeedVoucherAsync("ActiveV", 10m, isActive: true, isDeleted: false);
            var inactive = await SeedVoucherAsync("InactiveV", 12m, isActive: false, isDeleted: false);
            var deleted = await SeedVoucherAsync("DeletedV", 15m, isActive: true, isDeleted: true);

            var result = await _handler.Handle(new GetGlobalMetricsQuery(), CancellationToken.None);

            Assert.That(result.ActiveVouchers, Is.EqualTo(1));
        }

        [Test]
        public async Task SoldToday_Should_Count_Rows_Not_Quantities()
        {
            var v1 = await SeedVoucherAsync("VoucherA", 2m, isActive: true);
            var v2 = await SeedVoucherAsync("VoucherB", 3m, isActive: true);

            var utcNow = DateTime.UtcNow;
            var todayStartLocal = utcNow.Date.AddHours(4);
            var t = todayStartLocal.AddHours(2);

            await SeedPurchaseAsync(v1, 5, t);
            await SeedPurchaseAsync(v2, 10, t); 

            var result = await _handler.Handle(new GetGlobalMetricsQuery(), CancellationToken.None);

            Assert.That(result.SoldToday, Is.EqualTo(2));
            Assert.That(result.RevenueToday, Is.EqualTo((5 * 2m) + (10 * 3m)));
        }
    }
}