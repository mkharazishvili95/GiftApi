using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Statistics
{
    [TestFixture]
    public class BrandRedemptionLeaderboardHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IVoucherStatisticsRepository _repo = null!;
        private BrandRedemptionLeaderboardHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _repo = new VoucherStatisticsRepository(_db);
            _handler = new BrandRedemptionLeaderboardHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedAsync()
        {
            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "Alpha" };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "Beta" };
            var brandC = new GiftApi.Domain.Entities.Brand { Id = 3, Name = "Gamma" };

            var vouchers = new[]
            {
                new GiftApi.Domain.Entities.Voucher {
                    Id = Guid.NewGuid(), Title="A1", Description="", Amount=5, IsPercentage=false,
                    BrandId=brandA.Id, Brand=brandA, ValidMonths=6, Unlimited=false,
                    Quantity=10, Redeemed=5, SoldCount=10, IsActive=true, CreateDate=DateTime.UtcNow, IsDeleted=false
                },
                new GiftApi.Domain.Entities.Voucher {
                    Id = Guid.NewGuid(), Title="B1", Description="", Amount=5, IsPercentage=false,
                    BrandId=brandB.Id, Brand=brandB, ValidMonths=6, Unlimited=false,
                    Quantity=10, Redeemed=9, SoldCount=10, IsActive=true, CreateDate=DateTime.UtcNow, IsDeleted=false
                },
                new GiftApi.Domain.Entities.Voucher {
                    Id = Guid.NewGuid(), Title="C1", Description="", Amount=5, IsPercentage=false,
                    BrandId=brandC.Id, Brand=brandC, ValidMonths=6, Unlimited=false,
                    Quantity=8, Redeemed=0, SoldCount=8, IsActive=true, CreateDate=DateTime.UtcNow, IsDeleted=false
                },
                new GiftApi.Domain.Entities.Voucher {
                    Id = Guid.NewGuid(), Title="A2", Description="", Amount=5, IsPercentage=false,
                    BrandId=brandA.Id, Brand=brandA, ValidMonths=6, Unlimited=false,
                    Quantity=4, Redeemed=1, SoldCount=4, IsActive=true, CreateDate=DateTime.UtcNow, IsDeleted=false
                }
            };

            _db.Vouchers.AddRange(vouchers);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Calculate_RedemptionRate_And_Remaining()
        {
            await SeedAsync();
            var result = await _handler.Handle(new BrandRedemptionLeaderboardQuery(10), default);
            Assert.That(result.Items.Count, Is.EqualTo(3));

            var brandB = result.Items.Single(x => x.BrandName == "Beta");
            Assert.That(brandB.Sold, Is.EqualTo(10));
            Assert.That(brandB.Redeemed, Is.EqualTo(9));
            Assert.That(brandB.Remaining, Is.EqualTo(10 - 9));
            Assert.That(Math.Round(brandB.RedemptionRate, 2), Is.EqualTo(Math.Round(9m / 10m, 2)));

            var brandA = result.Items.Single(x => x.BrandName == "Alpha");
            Assert.That(brandA.Sold, Is.EqualTo(14));
            Assert.That(brandA.Redeemed, Is.EqualTo(6));
            Assert.That(brandA.Remaining, Is.EqualTo(8));
            Assert.That(Math.Round(brandA.RedemptionRate, 4), Is.EqualTo(Math.Round(6m / 14m, 4)));
        }

        [Test]
        public async Task Should_Order_By_RedemptionRate_Then_Redeemed_Then_Name()
        {
            await SeedAsync();
            var result = await _handler.Handle(new BrandRedemptionLeaderboardQuery(10), default);

            var orderedNames = result.Items.Select(i => i.BrandName).ToList();
            CollectionAssert.AreEqual(new[] { "Beta", "Alpha", "Gamma" }, orderedNames);
        }

        [Test]
        public async Task Should_Respect_Top_Limit()
        {
            await SeedAsync();
            var result = await _handler.Handle(new BrandRedemptionLeaderboardQuery(2), default);
            Assert.That(result.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Should_Default_To_10_When_Top_Less_Or_Equal_Zero()
        {
            await SeedAsync();
            var result = await _handler.Handle(new BrandRedemptionLeaderboardQuery(0), default);
            Assert.That(result.Items.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task Should_Handle_Brand_With_Zero_Sold_Gracefully()
        {
            await SeedAsync();

            var brandD = new GiftApi.Domain.Entities.Brand { Id = 4, Name = "Delta" };
            _db.Vouchers.Add(new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "D1",
                Description = "",
                Amount = 5,
                IsPercentage = false,
                BrandId = brandD.Id,
                Brand = brandD,
                ValidMonths = 6,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                SoldCount = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            });
            await _db.SaveChangesAsync();

            var result = await _handler.Handle(new BrandRedemptionLeaderboardQuery(10), default);
            var delta = result.Items.Single(x => x.BrandName == "Delta");
            Assert.That(delta.Sold, Is.EqualTo(0));
            Assert.That(delta.Redeemed, Is.EqualTo(0));
            Assert.That(delta.RedemptionRate, Is.EqualTo(0));
        }
    }
}