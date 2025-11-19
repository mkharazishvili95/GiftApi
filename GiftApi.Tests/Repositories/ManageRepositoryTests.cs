using System;
using System.Threading.Tasks;
using FluentAssertions;
using GiftApi.Domain.Entities;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using NUnit.Framework;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class ManageRepositoryTests
    {
        private ApplicationDbContext _db = null!;
        private ManageRepository _repository = null!;
        private Guid _voucherId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repository = new ManageRepository(_db);

            _voucherId = Guid.NewGuid();
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = _voucherId,
                Title = "Test Voucher",
                Description = "Desc",
                Amount = 10m,
                IsPercentage = false,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task<GiftApi.Domain.Entities.VoucherDeliveryInfo> SeedDeliveryInfoAsync(bool? isUsed)
        {
            var info = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = _voucherId,
                RecipientName = "Recipient",
                RecipientEmail = "recipient@example.com",
                RecipientPhone = "+995599000000",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Some Street 1",
                Message = "Hello",
                Quantity = 1,
                IsUsed = isUsed,
                CreateDate = DateTime.UtcNow
            };
            _db.VoucherDeliveryInfos.Add(info);
            await _db.SaveChangesAsync();
            return info;
        }

        [Test]
        public async Task ChangeUsedStatus_Should_Return_True_And_Set_IsUsed_When_Not_Used()
        {
            var info = await SeedDeliveryInfoAsync(false);

            var result = await _repository.ChangeUsedStatus(info.Id);

            result.Should().BeTrue();
            var refreshed = await _db.VoucherDeliveryInfos.FindAsync(info.Id);
            refreshed!.IsUsed.Should().BeTrue();
        }

        [Test]
        public async Task ChangeUsedStatus_Should_Handle_Null_IsUsed_As_Not_Used()
        {
            var info = await SeedDeliveryInfoAsync(null);

            var result = await _repository.ChangeUsedStatus(info.Id);

            result.Should().BeTrue();
            var refreshed = await _db.VoucherDeliveryInfos.FindAsync(info.Id);
            refreshed!.IsUsed.Should().BeTrue();
        }

        [Test]
        public async Task ChangeUsedStatus_Should_Return_False_When_Already_Used()
        {
            var info = await SeedDeliveryInfoAsync(true);

            var result = await _repository.ChangeUsedStatus(info.Id);

            result.Should().BeFalse();
            var refreshed = await _db.VoucherDeliveryInfos.FindAsync(info.Id);
            refreshed!.IsUsed.Should().BeTrue();
        }

        [Test]
        public async Task ChangeUsedStatus_Should_Return_False_When_Not_Found()
        {
            var result = await _repository.ChangeUsedStatus(Guid.NewGuid());

            result.Should().BeFalse();
        }
    }
}