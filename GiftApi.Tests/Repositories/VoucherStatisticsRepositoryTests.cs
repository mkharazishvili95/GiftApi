using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class VoucherStatisticsRepositoryTests
    {
        ApplicationDbContext _db = null!;
        VoucherStatisticsRepository _repo = null!;
        int _brandIdA;
        int _brandIdB;
        Guid _voucherA1;
        Guid _voucherA2;
        Guid _voucherB1;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new VoucherStatisticsRepository(_db);

            var brandA = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand A",
                CreateDate = DateTime.UtcNow.AddDays(-30),
                IsDeleted = false
            };
            var brandB = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand B",
                CreateDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false
            };
            _db.Brands.AddRange(brandA, brandB);
            await _db.SaveChangesAsync();
            _brandIdA = brandA.Id;
            _brandIdB = brandB.Id;

            var vA1 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher A1",
                Description = "Desc",
                Amount = 10,
                IsPercentage = false,
                BrandId = _brandIdA,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 20,
                IsActive = true,
                CreateDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false,
                SoldCount = 50
            };
            var vA2 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher A2",
                Description = "Desc",
                Amount = 15,
                IsPercentage = false,
                BrandId = _brandIdA,
                ValidMonths = 2,
                Unlimited = true,
                Quantity = 0,
                Redeemed = 5,
                IsActive = false,
                CreateDate = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false,
                SoldCount = 25
            };
            var vB1 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher B1",
                Description = "Desc",
                Amount = 20,
                IsPercentage = false,
                BrandId = _brandIdB,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 40,
                Redeemed = 10,
                IsActive = true,
                CreateDate = DateTime.UtcNow.AddDays(-3),
                IsDeleted = false,
                SoldCount = 30
            };
            _db.Vouchers.AddRange(vA1, vA2, vB1);
            await _db.SaveChangesAsync();
            _voucherA1 = vA1.Id;
            _voucherA2 = vA2.Id;
            _voucherB1 = vB1.Id;

            _db.VoucherDeliveryInfos.AddRange(
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vA1.Id,
                    Voucher = vA1,
                    Quantity = 5,
                    IsUsed = false,
                    CreateDate = DateTime.UtcNow.AddDays(-2),
                    RecipientName = "R1",
                    RecipientPhone = "555",
                    RecipientCity = "City",
                    RecipientAddress = "Addr"
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vA1.Id,
                    Voucher = vA1,
                    Quantity = 3,
                    IsUsed = true,
                    CreateDate = DateTime.UtcNow.AddDays(-1),
                    RecipientName = "R2",
                    RecipientPhone = "556",
                    RecipientCity = "City",
                    RecipientAddress = "Addr"
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vB1.Id,
                    Voucher = vB1,
                    Quantity = 2,
                    IsUsed = true,
                    CreateDate = DateTime.UtcNow.AddDays(-1),
                    RecipientName = "R3",
                    RecipientPhone = "557",
                    RecipientCity = "City",
                    RecipientAddress = "Addr"
                }
            );
            _db.VoucherRedeemAudits.AddRange(
                new GiftApi.Domain.Entities.VoucherRedeemAudit
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vA1.Id,
                    Action = "Redeem",
                    Quantity = 2,
                    PerformedAt = DateTime.UtcNow.AddDays(-1)
                },
                new GiftApi.Domain.Entities.VoucherRedeemAudit
                {
                    Id = Guid.NewGuid(),
                    VoucherId = vB1.Id,
                    Action = "Redeem",
                    Quantity = 1,
                    PerformedAt = DateTime.UtcNow.AddDays(-1)
                }
            );
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task GetVoucherUsageStatsAsync_Should_Filter_And_Aggregate()
        {
            var result = await _repo.GetVoucherUsageStatsAsync(_brandIdA, "A", includeInactive: true, orderBy: "sold", desc: true, CancellationToken.None);
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.All(i => i.BrandName == "Brand A").Should().BeTrue();
            result.TotalVouchers.Should().Be(3);
            result.TotalActiveVouchers.Should().Be(2);
            result.TotalSold.Should().Be(50 + 25);
            result.TotalRedeemed.Should().Be(20 + 5);
            result.TotalRemaining.Should().BeGreaterThan(0);
            result.AverageUsageRate.Should().BeGreaterThan(0);
            (result.Items.First().Sold >= result.Items.Last().Sold).Should().BeTrue();
        }

        [Test]
        public async Task GetVoucherUsageStatsAsync_Should_Order_By_Usage()
        {
            var result = await _repo.GetVoucherUsageStatsAsync(null, null, true, "usage", true, CancellationToken.None);
            var ordered = result.Items.Select(i => i.UsageRate).ToList();
            ordered.Should().BeInDescendingOrder();
        }

        [Test]
        public async Task GetExpiringSoonAsync_Should_Return_Expiring_Vouchers()
        {
            var list = await _repo.GetExpiringSoonAsync(null, 20, true, CancellationToken.None);
            list.Should().NotBeNull();
            list.Any(x => x.VoucherId == _voucherA1 || x.VoucherId == _voucherB1).Should().BeTrue();
            list.All(x => x.ExpiryDate <= DateTime.UtcNow.AddDays(20)).Should().BeTrue();
        }

        [Test]
        public async Task GetDailyUsageAsync_Should_Return_Rows_For_Days()
        {
            var rows = await _repo.GetDailyUsageAsync(null, 5, CancellationToken.None);
            rows.Should().NotBeNull();
            rows.Count.Should().Be(5);
            rows.Sum(r => r.Sold).Should().BeGreaterThan(0);
            rows.Sum(r => r.Redeemed).Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetBrandRedemptionLeaderboardAsync_Should_Order_By_Rate()
        {
            var board = await _repo.GetBrandRedemptionLeaderboardAsync(10, CancellationToken.None);
            board.Should().NotBeNull();
            board.Count.Should().Be(2);
            (board.First().RedemptionRate >= board.Last().RedemptionRate).Should().BeTrue();
            board.Select(b => b.BrandName).Should().Contain(new[] { "Brand A", "Brand B" });
        }
    }
}