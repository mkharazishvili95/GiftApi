using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Statistics
{
    [TestFixture]
    public class VoucherUsageTrendHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IVoucherStatisticsRepository _repo = null!;
        private VoucherUsageTrendHandler _handler = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _repo = new VoucherStatisticsRepository(_db);
            _handler = new VoucherUsageTrendHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedAsync()
        {
            var baseDate = DateTime.UtcNow.Date;
            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "BrandA" };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "BrandB" };

            var voucherA = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "VoucherA",
                Description = "",
                Amount = 10,
                IsPercentage = false,
                BrandId = brandA.Id,
                Brand = brandA,
                ValidMonths = 6,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                IsActive = true,
                CreateDate = baseDate.AddDays(-10),
                IsDeleted = false,
                SoldCount = 0
            };
            var voucherB = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "VoucherB",
                Description = "",
                Amount = 5,
                IsPercentage = false,
                BrandId = brandB.Id,
                Brand = brandB,
                ValidMonths = 3,
                Unlimited = false,
                Quantity = 50,
                Redeemed = 0,
                IsActive = true,
                CreateDate = baseDate.AddDays(-10),
                IsDeleted = false,
                SoldCount = 0
            };

            _db.Vouchers.AddRange(voucherA, voucherB);

            var deliveries = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherA.Id,
                    Voucher = voucherA,
                    RecipientName = "User1",
                    RecipientPhone = "111",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    Quantity = 2,
                    IsUsed = false,
                    CreateDate = baseDate.AddDays(-4)
                },

                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherA.Id,
                    Voucher = voucherA,
                    RecipientName = "User2",
                    RecipientPhone = "222",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    Quantity = 3,
                    IsUsed = false,
                    CreateDate = baseDate.AddDays(-3)
                },
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherB.Id,
                    Voucher = voucherB,
                    RecipientName = "User3",
                    RecipientPhone = "333",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    Quantity = 1,
                    IsUsed = false,
                    CreateDate = baseDate.AddDays(-3)
                },

                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherB.Id,
                    Voucher = voucherB,
                    RecipientName = "User4",
                    RecipientPhone = "444",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    Quantity = null,
                    IsUsed = false,
                    CreateDate = baseDate.AddDays(-1)
                },
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherA.Id,
                    Voucher = voucherA,
                    RecipientName = "User5",
                    RecipientPhone = "555",
                    RecipientCity = "City",
                    RecipientAddress = "Addr",
                    Quantity = 4,
                    IsUsed = false,
                    CreateDate = baseDate
                }
            };
            _db.VoucherDeliveryInfos.AddRange(deliveries);

            var audits = new List<VoucherRedeemAudit>
            {
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherA.Id,
                    DeliveryInfoId = deliveries[1].Id,
                    PerformedByUserId = Guid.NewGuid(),
                    Action = "Redeem",
                    PerformedAt = baseDate.AddDays(-3),
                    Quantity = 2,
                    PreviousIsUsed = false,
                    NewIsUsed = true
                },
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherB.Id,
                    DeliveryInfoId = deliveries[3].Id,
                    PerformedByUserId = Guid.NewGuid(),
                    Action = "Redeem",
                    PerformedAt = baseDate.AddDays(-1),
                    Quantity = 1,
                    PreviousIsUsed = false,
                    NewIsUsed = true
                },
                new() {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherA.Id,
                    DeliveryInfoId = deliveries[4].Id,
                    PerformedByUserId = Guid.NewGuid(),
                    Action = "Redeem",
                    PerformedAt = baseDate,
                    Quantity = 1,
                    PreviousIsUsed = false,
                    NewIsUsed = true
                }
            };
            _db.VoucherRedeemAudits.AddRange(audits);

            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Continuous_Days_With_Zeros()
        {
            await SeedAsync();
            var days = 5;
            var result = await _handler.Handle(new VoucherUsageTrendQuery(null, days), default);

            Assert.That(result.Points.Count, Is.EqualTo(days));
            var ordered = result.Points.Select(p => p.Day).OrderBy(d => d).ToList();
            CollectionAssert.AreEqual(ordered, result.Points.Select(p => p.Day).ToList());
        }

        [Test]
        public async Task Should_Aggregate_Sold_And_Redeemed_Correctly()
        {
            await SeedAsync();
            var baseDate = DateTime.UtcNow.Date;
            var res = await _handler.Handle(new VoucherUsageTrendQuery(null, 5), default);

            var dayMinus4 = res.Points.Single(p => p.Day == DateOnly.FromDateTime(baseDate.AddDays(-4)));
            var dayMinus3 = res.Points.Single(p => p.Day == DateOnly.FromDateTime(baseDate.AddDays(-3)));
            var dayMinus1 = res.Points.Single(p => p.Day == DateOnly.FromDateTime(baseDate.AddDays(-1)));
            var today = res.Points.Single(p => p.Day == DateOnly.FromDateTime(baseDate));

            Assert.That(dayMinus4.Sold, Is.EqualTo(2));
            Assert.That(dayMinus4.Redeemed, Is.EqualTo(0));

            Assert.That(dayMinus3.Sold, Is.EqualTo(4));
            Assert.That(dayMinus3.Redeemed, Is.EqualTo(2));

            Assert.That(dayMinus1.Sold, Is.EqualTo(1)); 
            Assert.That(dayMinus1.Redeemed, Is.EqualTo(1));

            Assert.That(today.Sold, Is.EqualTo(4));
            Assert.That(today.Redeemed, Is.EqualTo(1));

            Assert.That(res.TotalSold, Is.EqualTo(2 + 4 + 1 + 4));
            Assert.That(res.TotalRedeemed, Is.EqualTo(2 + 1 + 1));
        }

        [Test]
        public async Task Should_Filter_By_Brand()
        {
            await SeedAsync();
            var baseDate = DateTime.UtcNow.Date;

            var resA = await _handler.Handle(new VoucherUsageTrendQuery(1, 5), default);
            var todayA = resA.Points.Single(p => p.Day == DateOnly.FromDateTime(baseDate));
            Assert.That(resA.TotalSold, Is.EqualTo(2 + 3 + 4)); 
            Assert.That(resA.TotalRedeemed, Is.EqualTo(2 + 1)); 
            Assert.That(todayA.Sold, Is.EqualTo(4));

            var resB = await _handler.Handle(new VoucherUsageTrendQuery(2, 5), default);
            Assert.That(resB.TotalSold, Is.EqualTo(1 + 1));
            Assert.That(resB.TotalRedeemed, Is.EqualTo(1));
        }

        [Test]
        public async Task Should_Handle_Days_Less_Or_Equal_Zero_As_One()
        {
            await SeedAsync();
            var res = await _handler.Handle(new VoucherUsageTrendQuery(null, 0), default);
            Assert.That(res.Points.Count, Is.EqualTo(1));
        }
    }
}