using GiftApi.Application.Features.Manage.Voucher.Queries.UsageStats;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GiftApi.Tests.Statistics
{
    [TestFixture]
    public class VoucherUsageStatsHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IVoucherStatisticsRepository _repo = null!;
        private VoucherUsageStatsHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _repo = new VoucherStatisticsRepository(_db);
            _handler = new VoucherUsageStatsHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedAsync()
        {
            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "BrandA" };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "BrandB" };
            var now = DateTime.UtcNow;

            var vouchers = new List<GiftApi.Domain.Entities.Voucher>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "A_Limited",
                    Description = "",
                    Amount = 10,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 6,
                    Unlimited = false,
                    Quantity = 100,
                    Redeemed = 10,
                    SoldCount = 30,
                    IsActive = true,
                    CreateDate = now.AddDays(-5),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "A_Unlimited",
                    Description = "",
                    Amount = 5,
                    IsPercentage = false,
                    BrandId = brandA.Id,
                    Brand = brandA,
                    ValidMonths = 12,
                    Unlimited = true,
                    Quantity = 0,
                    Redeemed = 20,
                    SoldCount = 50,
                    IsActive = true,
                    CreateDate = now.AddDays(-3),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "B_Inactive",
                    Description = "",
                    Amount = 7,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 3,
                    Unlimited = false,
                    Quantity = 40,
                    Redeemed = 5,
                    SoldCount = 15,
                    IsActive = false,
                    CreateDate = now.AddDays(-2),
                    IsDeleted = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "DeletedVoucher",
                    Description = "",
                    Amount = 12,
                    IsPercentage = false,
                    BrandId = brandB.Id,
                    Brand = brandB,
                    ValidMonths = 1,
                    Unlimited = false,
                    Quantity = 10,
                    Redeemed = 2,
                    SoldCount = 4,
                    IsActive = true,
                    CreateDate = now.AddDays(-1),
                    IsDeleted = true
                }
            };

            _db.Vouchers.AddRange(vouchers);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Empty_For_No_Data()
        {
            var result = await _handler.Handle(new VoucherUsageStatsQuery(), default);

            Assert.That(result.Items.Count, Is.EqualTo(0));
            Assert.That(result.TotalVouchers, Is.EqualTo(0));
            Assert.That(result.TotalActiveVouchers, Is.EqualTo(0));
            Assert.That(result.TotalSold, Is.EqualTo(0));
            Assert.That(result.TotalRedeemed, Is.EqualTo(0));
            Assert.That(result.TotalRemaining, Is.EqualTo(0));
            Assert.That(result.AverageUsageRate, Is.EqualTo(0));
        }

        [Test]
        public async Task Should_Return_Only_Active_By_Default()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery(), default);

            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.All(x => x.IsActive), Is.True);
        }

        [Test]
        public async Task Should_Include_Inactive_When_Requested()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true }, default);

            Assert.That(result.Items.Count, Is.EqualTo(3));
            Assert.That(result.Items.Any(x => !x.IsActive), Is.True);
        }

        [Test]
        public async Task Should_Filter_By_Brand()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { BrandId = 1, IncludeInactive = true }, default);

            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.All(x => x.BrandName == "BrandA"));
        }

        [Test]
        public async Task Should_Filter_By_Search_Title()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { Search = "limited", IncludeInactive = true }, default);

            Assert.That(result.Items.Count, Is.EqualTo(2));
            Assert.That(result.Items.All(x =>
                x.Title.Contains("Limited", StringComparison.OrdinalIgnoreCase)));
        }


        [Test]
        public async Task Should_Filter_By_Search_Brand()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { Search = "brandb", IncludeInactive = true }, default);

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items[0].BrandName, Is.EqualTo("BrandB"));
        }

        [Test]
        public async Task Should_Order_By_Sold_Descending()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true, OrderBy = "sold", Desc = true }, default);

            var sold = result.Items.Select(x => x.Sold).ToList();
            CollectionAssert.AreEqual(sold.OrderByDescending(x => x).ToList(), sold);
        }

        [Test]
        public async Task Should_Order_By_Sold_Ascending()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true, OrderBy = "sold", Desc = false }, default);

            var sold = result.Items.Select(x => x.Sold).ToList();
            CollectionAssert.AreEqual(sold.OrderBy(x => x).ToList(), sold);
        }

        [Test]
        public async Task Should_Calculate_Remaining_For_Limited_And_Unlimited()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true }, default);

            var limited = result.Items.First(x => x.Title == "A_Limited");
            var unlimited = result.Items.First(x => x.Title == "A_Unlimited");

            Assert.That(limited.Remaining, Is.EqualTo(100 - 10));
            Assert.That(unlimited.Remaining, Is.EqualTo(50 - 20));
        }

        [Test]
        public async Task Should_Set_ExpiryDate_Correctly()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true }, default);

            var limited = result.Items.First(x => x.Title == "A_Limited");
            var unlimited = result.Items.First(x => x.Title == "A_Unlimited");

            Assert.NotNull(limited.ExpiryDate);
            Assert.That(unlimited.ExpiryDate, Is.Null);
        }

        [Test]
        public async Task Should_Compute_Totals_And_AverageUsageRate()
        {
            await SeedAsync();
            var result = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true }, default);

            Assert.That(result.TotalVouchers, Is.EqualTo(4));
            Assert.That(result.TotalActiveVouchers, Is.EqualTo(2));

            var expectedSold = 30 + 50 + 15;       
            var expectedRedeemed = 10 + 20 + 5;
            var expectedRemaining = (100 - 10) + (50 - 20) + (40 - 5);

            Assert.That(result.TotalSold, Is.EqualTo(expectedSold));
            Assert.That(result.TotalRedeemed, Is.EqualTo(expectedRedeemed));
            Assert.That(result.TotalRemaining, Is.EqualTo(expectedRemaining));

            var usageRates = new[]
            {
                10m / 30m,
                20m / 50m,
                5m / 15m
            };
            var avg = Math.Round(usageRates.Average(), 4);
            Assert.That(result.AverageUsageRate, Is.EqualTo(avg));
        }


        [Test]
        public async Task Should_Order_By_UsageRate()
        {
            await SeedAsync();
            var descResult = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true, OrderBy = "usage", Desc = true }, default);
            var ascResult = await _handler.Handle(new VoucherUsageStatsQuery { IncludeInactive = true, OrderBy = "usage", Desc = false }, default);

            var descRates = descResult.Items.Select(x => x.UsageRate).ToList();
            var ascRates = ascResult.Items.Select(x => x.UsageRate).ToList();

            CollectionAssert.AreEqual(descRates.OrderByDescending(x => x).ToList(), descRates);
            CollectionAssert.AreEqual(ascRates.OrderBy(x => x).ToList(), ascRates);
        }
    }
}