using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Purchase
{
    [TestFixture]
    public class RedeemPurchaseHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IPurchaseRepository _repo = null!;
        private RedeemPurchaseHandler _handler = null!;
        private Guid _voucherId;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Redeem Voucher",
                Description = "Desc",
                Amount = 20,
                IsPercentage = false,
                Brand = new GiftApi.Domain.Entities.Brand { Id = 55, Name = "Redeem Brand" },
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _voucherId = voucher.Id;
            _db.Vouchers.Add(voucher);

            var delivery = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = _voucherId,
                RecipientName = "User",
                RecipientPhone = "555",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Addr",
                Quantity = 1,
                IsUsed = false
            };
            _db.VoucherDeliveryInfos.Add(delivery);
            await _db.SaveChangesAsync();

            _repo = new PurchaseRepository(_db);
            _handler = new RedeemPurchaseHandler(_repo);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Redeem_Successfully()
        {
            var entity = _db.VoucherDeliveryInfos.First();
            var cmd = new RedeemPurchaseCommand
            {
                DeliveryInfoId = entity.Id,
                PerformedByUserId = Guid.NewGuid()
            };

            var result = await _handler.Handle(cmd, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Redeemed successfully", result.Message);
            Assert.NotNull(result.UsedDate);

            var updated = _db.VoucherDeliveryInfos.First();
            Assert.IsTrue(updated.IsUsed);
            Assert.NotNull(updated.UsedDate);

            var voucher = _db.Vouchers.First();
            Assert.AreEqual(1, voucher.Redeemed);
        }

        [Test]
        public async Task Should_Not_Redeem_Twice()
        {
            var entity = _db.VoucherDeliveryInfos.First();
            
            await _handler.Handle(new RedeemPurchaseCommand
            {
                DeliveryInfoId = entity.Id,
                PerformedByUserId = Guid.NewGuid()
            }, default);

            var second = await _handler.Handle(new RedeemPurchaseCommand
            {
                DeliveryInfoId = entity.Id,
                PerformedByUserId = Guid.NewGuid()
            }, default);

            Assert.IsFalse(second.Success);
            Assert.AreEqual("Already redeemed", second.Message);
        }

        [Test]
        public async Task Should_Fail_When_NotFound()
        {
            var cmd = new RedeemPurchaseCommand
            {
                DeliveryInfoId = Guid.NewGuid(),
                PerformedByUserId = Guid.NewGuid()
            };

            var result = await _handler.Handle(cmd, default);
            Assert.IsFalse(result.Success);
            Assert.AreEqual("Purchase not found", result.Message);
        }
    }
}