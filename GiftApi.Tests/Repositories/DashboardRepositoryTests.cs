using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class DashboardRepositoryTests
    {
        ApplicationDbContext _db = null!;
        DashboardRepository _repo = null!;
        Guid _voucher1Id;
        Guid _voucher2Id;
        Guid _voucher3Id;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new DashboardRepository(_db);

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand A",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var v1 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 1",
                Description = "Desc",
                Amount = 10,
                IsPercentage = false,
                BrandId = brand.Id,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 2,
                IsActive = true,
                CreateDate = DateTime.UtcNow.AddDays(-10),
                IsDeleted = false,
                SoldCount = 5
            };
            var v2 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 2",
                Description = "Desc",
                Amount = 15,
                IsPercentage = false,
                BrandId = brand.Id,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 20,
                Redeemed = 5,
                IsActive = true,
                CreateDate = DateTime.UtcNow.AddDays(-20),
                IsDeleted = false,
                SoldCount = 12
            };
            var v3 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 3 (Inactive)",
                Description = "Desc",
                Amount = 25,
                IsPercentage = false,
                BrandId = brand.Id,
                ValidMonths = 2,
                Unlimited = true,
                Quantity = 0,
                Redeemed = 0,
                IsActive = false,
                CreateDate = DateTime.UtcNow.AddDays(-5),
                IsDeleted = false,
                SoldCount = 0
            };
            _db.Vouchers.AddRange(v1, v2, v3);
            await _db.SaveChangesAsync();
            _voucher1Id = v1.Id;
            _voucher2Id = v2.Id;
            _voucher3Id = v3.Id;

            var user1 = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "U1",
                LastName = "L1",
                DateOfBirth = DateTime.UtcNow.AddYears(-30),
                IdentificationNumber = "12345678901",
                Email = "u1@example.com",
                PhoneNumber = "+995599000001",
                UserName = "user1",
                Password = "hash",
                RegisterDate = DateTime.UtcNow,
                Balance = 0,
                Type = GiftApi.Domain.Enums.User.UserType.User,
                EmailVerified = true
            };
            var user2 = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "U2",
                LastName = "L2",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                IdentificationNumber = "12345678902",
                Email = "u2@example.com",
                PhoneNumber = "+995599000002",
                UserName = "user2",
                Password = "hash",
                RegisterDate = DateTime.UtcNow,
                Balance = 0,
                Type = GiftApi.Domain.Enums.User.UserType.User,
                EmailVerified = false
            };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();

            var today = DateTime.UtcNow.Date.AddHours(4);
            var purchase1 = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = v1.Id,
                Voucher = v1,
                RecipientName = "R1",
                RecipientPhone = "123",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 2,
                IsUsed = true,
                UsedDate = DateTime.UtcNow,
                CreateDate = today.AddHours(1)
            };
            var purchase2 = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = v2.Id,
                Voucher = v2,
                RecipientName = "R2",
                RecipientPhone = "456",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 1,
                IsUsed = false,
                CreateDate = today.AddHours(2)
            };
            var oldPurchase = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = v2.Id,
                Voucher = v2,
                RecipientName = "Old",
                RecipientPhone = "789",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 3,
                IsUsed = true,
                CreateDate = today.AddDays(-2)
            };
            _db.VoucherDeliveryInfos.AddRange(purchase1, purchase2, oldPurchase);
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task GetGlobalMetricsAsync_Should_Return_Today_Metrics()
        {
            var metrics = await _repo.GetGlobalMetricsAsync(CancellationToken.None);
            metrics.Should().NotBeNull();
            metrics.UsersCount.Should().Be(2);
            metrics.ActiveVouchers.Should().Be(2);
            metrics.SoldToday.Should().Be(2); 
            metrics.RevenueToday.Should().Be(10 * 2 + 15 * 1);
        }

        [Test]
        public async Task GetSummaryAsync_Should_Return_Summary_With_TopBrands()
        {
            var summary = await _repo.GetSummaryAsync(30, 5, CancellationToken.None);
            summary.Should().NotBeNull();
            summary.UsersTotal.Should().Be(2);
            summary.ActiveVouchers.Should().Be(2);
            summary.SoldVouchersTotal.Should().Be(5 + 12 + 0);
            summary.RedeemedTotal.Should().Be(2 + 5 + 0);
            summary.UnusedVouchersTotal.Should().Be((10 - 2) + (20 - 5) + 0);
            summary.TotalRevenueEstimate.Should().Be((2 * 10) + (5 * 15));
            summary.PurchasesTotal.Should().Be(3);
            summary.UsedPurchases.Should().Be(2);
            summary.UnusedPurchases.Should().Be(1);
            summary.ExpiringSoonCount.Should().BeGreaterThanOrEqualTo(1);
            summary.TopBrands.Count.Should().Be(1);
            var top = summary.TopBrands.First();
            top.BrandName.Should().Be("Brand A");
            top.TotalSoldCount.Should().Be(5 + 12 + 0);
            top.VouchersCount.Should().Be(3);
        }
    }
}