using GiftApi.Application.Features.Manage.Dashboard.Queries.GetSummary;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Dashboard
{
    [TestFixture]
    public class DashboardSummaryHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IDashboardRepository _repo = null!;
        private DashboardSummaryHandler _handler = null!;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _repo = new DashboardRepository(_db);
            _handler = new DashboardSummaryHandler(_repo);

            _db.Users.AddRange(
                new GiftApi.Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test",
                    LastName = "User1",
                    IdentificationNumber = "ID123456",
                    Email = "u1@test.com",
                    PhoneNumber = "555111111",
                    UserName = "user1",
                    Password = "hashedpwd",
                    Balance = 100,
                    Type = GiftApi.Domain.Enums.User.UserType.User
                },
                new GiftApi.Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test",
                    LastName = "User2",
                    IdentificationNumber = "ID654321",
                    Email = "u2@test.com",
                    PhoneNumber = "555222222",
                    UserName = "user2",
                    Password = "hashedpwd",
                    Balance = 50,
                    Type = GiftApi.Domain.Enums.User.UserType.User
                }
            );

            var now = DateTime.UtcNow;

            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "BrandA", IsDeleted = false, CategoryId = 0, CreateDate = now };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "BrandB", IsDeleted = false, CategoryId = 0, CreateDate = now };
            var brandC = new GiftApi.Domain.Entities.Brand { Id = 3, Name = "BrandC", IsDeleted = false, CategoryId = 0, CreateDate = now };
            _db.Brands.AddRange(brandA, brandB, brandC);

            var vouchers = new List<GiftApi.Domain.Entities.Voucher>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Soon1",
                    Description = "",
                    Amount = 20,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 100,
                    Redeemed = 10,
                    SoldCount = 15,
                    IsActive = true,
                    CreateDate = now.AddDays(-15),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Later1",
                    Description = "",
                    Amount = 30,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 2,
                    Unlimited = false,
                    Quantity = 50,
                    Redeemed = 5,
                    SoldCount = 7,
                    IsActive = true,
                    CreateDate = now.AddDays(-10),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Percent1",
                    Description = "",
                    Amount = 15,
                    IsPercentage = true,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 80,
                    Redeemed = 8,
                    SoldCount = 12,
                    IsActive = true,
                    CreateDate = now.AddDays(-5),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Unlimited1",
                    Description = "",
                    Amount = 40,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 6,
                    Unlimited = true,
                    Quantity = 0,
                    Redeemed = 0,
                    SoldCount = 0,
                    IsActive = true,
                    CreateDate = now.AddDays(-1),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "BrandC_V1",
                    Description = "",
                    Amount = 10,
                    IsPercentage = false,
                    BrandId = brandC.Id,
                    Brand = brandC,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 30,
                    Redeemed = 3,
                    SoldCount = 9,
                    IsActive = true,
                    CreateDate = now.AddDays(-2),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "BrandC_V2",
                    Description = "",
                    Amount = 12,
                    IsPercentage = false,
                    BrandId = brandC.Id,
                    Brand = brandC,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 30,
                    Redeemed = 6,
                    SoldCount = 11,
                    IsActive = true,
                    CreateDate = now.AddDays(-3),
                    IsDeleted = false
                }
            };
            _db.Vouchers.AddRange(vouchers);

            _db.VoucherDeliveryInfos.AddRange(
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vouchers[0].Id,
                    RecipientName = "R1",
                    RecipientPhone = "111",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    IsUsed = true
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vouchers[1].Id,
                    RecipientName = "R2",
                    RecipientPhone = "222",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    IsUsed = false
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vouchers[2].Id,
                    RecipientName = "R3",
                    RecipientPhone = "333",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    IsUsed = true
                }
            );

            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_Correct_Summary_CoreMetrics()
        {
            var query = new DashboardSummaryQuery { ExpiringInDays = 30, TopBrands = 3 };
            var result = await _handler.Handle(query, default);

            Assert.That(result.UsersTotal, Is.EqualTo(2));
            Assert.That(result.ActiveVouchers, Is.EqualTo(6));
            Assert.That(result.SoldVouchersTotal, Is.EqualTo(15 + 7 + 12 + 0 + 9 + 11));
            Assert.That(result.RedeemedTotal, Is.EqualTo(10 + 5 + 8 + 0 + 3 + 6));
            Assert.That(result.UnusedVouchersTotal, Is.EqualTo(
                (100 - 10) + (50 - 5) + (80 - 8) + (0 - 0) + (30 - 3) + (30 - 6)));
            Assert.That(result.TotalRevenueEstimate, Is.EqualTo(
                10 * 20 + 5 * 30 + 0 + 3 * 10 + 6 * 12));
            Assert.That(result.PurchasesTotal, Is.EqualTo(3));
            Assert.That(result.UsedPurchases, Is.EqualTo(2));
            Assert.That(result.UnusedPurchases, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Calculate_ExpiringSoon_Correctly()
        {
            var query = new DashboardSummaryQuery { ExpiringInDays = 30 };
            var result = await _handler.Handle(query, default);
            Assert.That(result.ExpiringSoonCount, Is.EqualTo(4));
        }

        [Test]
        public async Task Should_Order_TopBrands_By_SoldCount_Then_VoucherCount()
        {
            var query = new DashboardSummaryQuery { TopBrands = 3 };
            var result = await _handler.Handle(query, default);

            Assert.That(result.TopBrands.Count, Is.EqualTo(3));
            Assert.That(result.TopBrands[0].BrandName, Is.EqualTo("BrandA"));
            Assert.That(result.TopBrands[1].BrandName, Is.EqualTo("BrandC"));
            Assert.That(result.TopBrands[2].BrandName, Is.EqualTo("BrandB"));
            Assert.That(result.TopBrands[0].TotalSoldCount, Is.EqualTo(22));
            Assert.That(result.TopBrands[1].TotalSoldCount, Is.EqualTo(20));
            Assert.That(result.TopBrands[2].TotalSoldCount, Is.EqualTo(12));
        }

        [Test]
        public async Task Should_Handle_Empty_Data()
        {
            _db.VoucherDeliveryInfos.RemoveRange(_db.VoucherDeliveryInfos);
            _db.Vouchers.RemoveRange(_db.Vouchers);
            _db.Users.RemoveRange(_db.Users);
            _db.Brands.RemoveRange(_db.Brands);
            await _db.SaveChangesAsync();

            var query = new DashboardSummaryQuery { ExpiringInDays = 30, TopBrands = 5 };
            var result = await _handler.Handle(query, default);

            Assert.That(result.UsersTotal, Is.EqualTo(0));
            Assert.That(result.ActiveVouchers, Is.EqualTo(0));
            Assert.That(result.SoldVouchersTotal, Is.EqualTo(0));
            Assert.That(result.RedeemedTotal, Is.EqualTo(0));
            Assert.That(result.UnusedVouchersTotal, Is.EqualTo(0));
            Assert.That(result.TotalRevenueEstimate, Is.EqualTo(0));
            Assert.That(result.PurchasesTotal, Is.EqualTo(0));
            Assert.That(result.UsedPurchases, Is.EqualTo(0));
            Assert.That(result.UnusedPurchases, Is.EqualTo(0));
            Assert.That(result.ExpiringSoonCount, Is.EqualTo(0));
            Assert.That(result.TopBrands, Is.Empty);
        }

        [Test]
        public async Task Should_Exclude_Percentage_Vouchers_From_Revenue()
        {
            var query = new DashboardSummaryQuery { ExpiringInDays = 30 };
            var result = await _handler.Handle(query, default);

            var manualRevenue = _db.Vouchers
                .Where(v => !v.IsDeleted && !v.IsPercentage)
                .Sum(v => v.Redeemed * v.Amount);

            Assert.That(result.TotalRevenueEstimate, Is.EqualTo(manualRevenue));

            var withPercentage = _db.Vouchers
                .Where(v => !v.IsDeleted)
                .Sum(v => v.Redeemed * v.Amount);

            Assert.That(withPercentage, Is.GreaterThan(result.TotalRevenueEstimate));
        }
    }
}