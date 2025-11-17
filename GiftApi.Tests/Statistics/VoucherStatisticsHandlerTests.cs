using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Statistics
{
    [TestFixture]
    public class VoucherStatisticsHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IVoucherStatisticsRepository _repo = null!;
        private VoucherStatisticsHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _repo = new VoucherStatisticsRepository(_db);
            _handler = new VoucherStatisticsHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedAsync()
        {
            var now = DateTime.UtcNow;

            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "BrandA", CategoryId = 0, CreateDate = now };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "BrandB", CategoryId = 0, CreateDate = now };
            _db.Brands.AddRange(brandA, brandB);

            var vouchers = new List<GiftApi.Domain.Entities.Voucher>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "SoonExpiring",
                    Description = "",
                    Amount = 10,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 1,                    
                    Unlimited = false,
                    Quantity = 100,
                    Redeemed = 20,
                    SoldCount = 40,
                    IsActive = true,
                    CreateDate = now.AddDays(-10),      
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "LowStock",
                    Description = "",
                    Amount = 15,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 6,
                    Unlimited = false,
                    Quantity = 10,
                    Redeemed = 8,                       
                    SoldCount = 12,
                    IsActive = true,
                    CreateDate = now.AddDays(-5),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "UnlimitedHighSold",
                    Description = "",
                    Amount = 5,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 12,
                    Unlimited = true,
                    Quantity = 0,
                    Redeemed = 40,
                    SoldCount = 100,
                    IsActive = true,
                    CreateDate = now.AddDays(-3),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "InactiveVoucher",
                    Description = "",
                    Amount = 7,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 2,
                    Unlimited = false,
                    Quantity = 50,
                    Redeemed = 5,
                    SoldCount = 25,
                    IsActive = false,
                    CreateDate = now.AddDays(-4),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "AlreadyExpired",
                    Description = "",
                    Amount = 9,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 30,
                    Redeemed = 10,
                    SoldCount = 20,
                    IsActive = true,
                    CreateDate = now.AddDays(-40),       
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "OutsideDateRange",
                    Description = "",
                    Amount = 11,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 3,
                    Unlimited = false,
                    Quantity = 60,
                    Redeemed = 5,
                    SoldCount = 18,
                    IsActive = true,
                    CreateDate = now.AddDays(-120),      
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "DeletedVoucher",
                    Description = "",
                    Amount = 13,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 15,
                    Redeemed = 2,
                    SoldCount = 5,
                    IsActive = true,
                    CreateDate = now.AddDays(-2),
                    IsDeleted = true
                }
            };

            _db.Vouchers.AddRange(vouchers);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Empty_For_No_Data()
        {
            var result = await _handler.Handle(new VoucherStatisticsQuery(), default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(0));
            Assert.That(result.TopSold, Is.Empty);
            Assert.That(result.LowStock, Is.Empty);
            Assert.That(result.ExpiringSoon, Is.Empty);
        }

        [Test]
        public async Task Should_Exclude_Inactive_By_Default()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery(), default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(5)); 
            Assert.That(result.TopSold.Any(x => x.Title == "InactiveVoucher"), Is.False);
        }

        [Test]
        public async Task Should_Include_Inactive_When_Requested()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { IncludeInactive = true }, default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(6)); 
            Assert.That(result.TopSold.Any(x => x.Title == "InactiveVoucher"), Is.True);
        }

        [Test]
        public async Task Should_Order_TopSold_By_SoldCount_And_Respect_Take()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { TopSoldTake = 2, IncludeInactive = true }, default);

            Assert.That(result.TopSold.Count, Is.EqualTo(2));
            var soldCounts = result.TopSold.Select(x => x.SoldCount).ToList();
            CollectionAssert.AreEqual(soldCounts.OrderByDescending(x => x).ToList(), soldCounts);
            Assert.That(result.TopSold[0].Title, Is.EqualTo("UnlimitedHighSold"));
        }

        [Test]
        public async Task Should_Detect_LowStock_Vouchers()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { LowStockThreshold = 5 }, default);

            Assert.That(result.LowStock.Count, Is.EqualTo(1));
            var item = result.LowStock.First();
            Assert.That(item.Title, Is.EqualTo("LowStock"));
            Assert.That(item.Unlimited, Is.False);
            Assert.That(item.Remaining, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Not_Include_Unlimited_In_LowStock()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { LowStockThreshold = 200 }, default);
            Assert.That(result.LowStock.Any(x => x.Unlimited), Is.False);
        }

        [Test]
        public async Task Should_Calculate_Remaining_Correctly_For_Limited_And_Unlimited()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { IncludeInactive = true }, default);

            var unlimited = result.TopSold.First(x => x.Title == "UnlimitedHighSold");
            var limited = result.TopSold.First(x => x.Title == "SoonExpiring");

            Assert.That(unlimited.Unlimited, Is.True);
            Assert.That(unlimited.Remaining, Is.EqualTo(100 - 40)); 
            Assert.That(limited.Unlimited, Is.False);
            Assert.That(limited.Remaining, Is.EqualTo(100 - 20));  
        }

        [Test]
        public async Task Should_Detect_ExpiringSoon_Vouchers()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { ExpiringInDays = 25 }, default);

            Assert.That(result.ExpiringSoon.Any(x => x.Title == "SoonExpiring"), Is.True);
            Assert.That(result.ExpiringSoon.Any(x => x.Title == "AlreadyExpired"), Is.False);
            Assert.That(result.ExpiringSoon.All(x => !x.Unlimited), Is.True);
        }

        [Test]
        public async Task Should_Filter_By_Brand()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery { BrandId = 1 }, default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(3)); 
            Assert.That(result.TopSold.All(x => x.BrandName == "BrandA"), Is.True);
        }

        [Test]
        public async Task Should_Filter_By_DateRange()
        {
            await SeedAsync();
            var from = DateTime.UtcNow.AddDays(-20);
            var to = DateTime.UtcNow.AddDays(-15);
            var result = await _handler.Handle(new VoucherStatisticsQuery
            {
                FromUtc = from,
                ToUtc = to,
                IncludeInactive = true
            }, default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(0));
            Assert.That(result.TopSold, Is.Empty);
        }

        [Test]
        public async Task Should_Apply_DateRange_Partially()
        {
            await SeedAsync();
            var from = DateTime.UtcNow.AddDays(-12);
            var to = DateTime.UtcNow.AddDays(-2);
            var result = await _handler.Handle(new VoucherStatisticsQuery
            {
                FromUtc = from,
                ToUtc = to
            }, default);

            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(3));
            Assert.That(result.TopSold.All(x => x.CreateDate >= from && x.CreateDate <= to), Is.True);
        }

        [Test]
        public async Task Should_Return_Correct_TotalVouchersConsidered()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherStatisticsQuery(), default);
            Assert.That(result.TotalVouchersConsidered, Is.EqualTo(5));
        }
    }
}