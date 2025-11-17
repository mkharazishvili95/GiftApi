using GiftApi.Application.Features.Manage.Voucher.Queries.Statistics;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GiftApi.Tests.Statistics
{
    [TestFixture]
    public class ExpiringSoonVouchersHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IVoucherStatisticsRepository _repo = null!;
        private ExpiringSoonVouchersHandler _handler = null!;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _now = DateTime.UtcNow;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _repo = new VoucherStatisticsRepository(_db);
            _handler = new ExpiringSoonVouchersHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedAsync()
        {
            var brandA = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "BrandA", CategoryId = 0, CreateDate = _now };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "BrandB", CategoryId = 0, CreateDate = _now };
            _db.Brands.AddRange(brandA, brandB);

            GiftApi.Domain.Entities.Voucher V(string title, int brandId, int validMonths, bool unlimited,
                int quantity, int redeemed, int sold, bool active, int daysAgo, bool deleted = false)
            {
                var brand = brandId == brandA.Id ? brandA : brandB;
                return new GiftApi.Domain.Entities.Voucher
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = "",
                    Amount = 10,
                    IsPercentage = false,
                    BrandId = brandId,
                    Brand = brand,
                    ValidMonths = validMonths,
                    Unlimited = unlimited,
                    Quantity = quantity,
                    Redeemed = redeemed,
                    SoldCount = sold,
                    IsActive = active,
                    CreateDate = _now.AddDays(-daysAgo),
                    IsDeleted = deleted
                };
            }

            var vouchers = new[]
            {
                V("SoonExpiringA", brandA.Id, 1, false, 100, 20, 40, true, 20),
                V("VerySoon", brandA.Id, 1, false, 50, 10, 25, true, 28),
                V("AlreadyExpired", brandA.Id, 1, false, 30, 5, 10, true, 40),
                V("UnlimitedVoucher", brandB.Id, 6, true, 0, 15, 30, true, 5),
                V("InactiveSoonExpiring", brandB.Id, 1, false, 70, 10, 20, false, 25),
                V("FarExpiry", brandB.Id, 2, false, 80, 5, 15, true, 5),
                V("DeletedVoucher", brandA.Id, 1, false, 40, 2, 5, true, 10, deleted: true)
            };

            _db.Vouchers.AddRange(vouchers);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_Return_Empty_When_No_Data()
        {
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 30 }, default);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task Should_Return_Only_ExpiringWithin_Days()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 15 }, default);

            Assert.That(result.Any(v => v.Title == "SoonExpiringA"), Is.True); 
            Assert.That(result.Any(v => v.Title == "VerySoon"), Is.True);     
            Assert.That(result.Any(v => v.Title == "FarExpiry"), Is.False);
        }

        [Test]
        public async Task Should_Exclude_AlreadyExpired()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            Assert.That(result.Any(v => v.Title == "AlreadyExpired"), Is.False);
        }

        [Test]
        public async Task Should_Exclude_Unlimited()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            Assert.That(result.Any(v => v.Title == "UnlimitedVoucher"), Is.False);
        }

        [Test]
        public async Task Should_Exclude_Inactive_By_Default()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            Assert.That(result.Any(v => v.Title == "InactiveSoonExpiring"), Is.False);
        }

        [Test]
        public async Task Should_Include_Inactive_When_Requested()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60, IncludeInactive = true }, default);
            Assert.That(result.Any(v => v.Title == "InactiveSoonExpiring"), Is.True);
        }

        [Test]
        public async Task Should_Filter_By_Brand()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60, BrandId = 1 }, default);
            Assert.That(result.All(v => v.BrandName == "BrandA"), Is.True);
        }

        [Test]
        public async Task Should_Order_By_ExpiryDate_Ascending()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            var ordered = result.OrderBy(x => x.ExpiryDate).Select(x => x.VoucherId).ToList();
            var original = result.Select(x => x.VoucherId).ToList();
            CollectionAssert.AreEqual(ordered, original);
        }

        [Test]
        public async Task Should_Not_Return_Deleted()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            Assert.That(result.Any(v => v.Title == "DeletedVoucher"), Is.False);
        }

        [Test]
        public async Task Should_Calculate_Remaining_Correctly()
        {
            await SeedAsync();
            var result = await _handler.Handle(new ExpiringSoonVouchersQuery { Days = 60 }, default);
            var soon = result.First(v => v.Title == "SoonExpiringA");
            Assert.That(soon.Remaining, Is.EqualTo(100 - 20));
        }
    }
}