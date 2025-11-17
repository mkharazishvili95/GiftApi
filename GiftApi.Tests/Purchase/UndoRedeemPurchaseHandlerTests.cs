using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.Redeem;
using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.UndoRedeem;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Purchase
{
    [TestFixture]
    public class UndoRedeemPurchaseHandlerTests
    {
        ApplicationDbContext _db;
        IPurchaseRepository _purchaseRepository;
        RedeemPurchaseHandler _redeemHandler;
        UndoRedeemPurchaseHandler _undoHandler;

        [SetUp]
        public void Setup()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new ApplicationDbContext(opts);
            _purchaseRepository = new PurchaseRepository(_db);
            _redeemHandler = new RedeemPurchaseHandler(_purchaseRepository);
            _undoHandler = new UndoRedeemPurchaseHandler(_purchaseRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_NotFound_When_DeliveryInfo_Not_Exist()
        {
            var cmd = new UndoRedeemPurchaseCommand
            {
                DeliveryInfoId = Guid.NewGuid(),
                PerformedByUserId = Guid.NewGuid()
            };
            var res = await _undoHandler.Handle(cmd, CancellationToken.None);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(404, res.StatusCode);
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Not_Redeemed()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "V",
                Description = "D",
                Amount = 10,
                ValidMonths = 6,
                CreateDate = DateTime.UtcNow,
                IsActive = true
            };
            _db.Vouchers.Add(voucher);

            var info = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                RecipientName = "R",
                RecipientPhone = "123",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                IsUsed = false,
                Quantity = 2
            };
            _db.VoucherDeliveryInfos.Add(info);
            await _db.SaveChangesAsync();

            var cmd = new UndoRedeemPurchaseCommand
            {
                DeliveryInfoId = info.Id,
                PerformedByUserId = Guid.NewGuid()
            };
            var res = await _undoHandler.Handle(cmd, CancellationToken.None);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(400, res.StatusCode);
        }

        [Test]
        public async Task Should_Undo_Redeem_Successfully()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "V",
                Description = "D",
                Amount = 25,
                ValidMonths = 6,
                CreateDate = DateTime.UtcNow,
                IsActive = true,
                Redeemed = 0
            };
            _db.Vouchers.Add(voucher);

            var info = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                RecipientName = "R",
                RecipientPhone = "123",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 3,
                IsUsed = false
            };
            _db.VoucherDeliveryInfos.Add(info);
            await _db.SaveChangesAsync();

            var redeemRes = await _redeemHandler.Handle(new RedeemPurchaseCommand
            {
                DeliveryInfoId = info.Id,
                PerformedByUserId = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.IsTrue(redeemRes.Success);

            var beforeUndoVoucher = await _db.Vouchers.FirstAsync(v => v.Id == voucher.Id);
            Assert.AreEqual(3, beforeUndoVoucher.Redeemed);

            var undoRes = await _undoHandler.Handle(new UndoRedeemPurchaseCommand
            {
                DeliveryInfoId = info.Id,
                PerformedByUserId = Guid.NewGuid()
            }, CancellationToken.None);

            Assert.IsTrue(undoRes.Success);
            Assert.AreEqual(200, undoRes.StatusCode);
            Assert.IsFalse(undoRes.IsUsed);
            Assert.IsNull(undoRes.UsedDate);

            var afterUndoInfo = await _db.VoucherDeliveryInfos.FirstAsync(x => x.Id == info.Id);
            Assert.IsFalse(afterUndoInfo.IsUsed ?? false);
            Assert.IsNull(afterUndoInfo.UsedDate);

            var afterUndoVoucher = await _db.Vouchers.FirstAsync(v => v.Id == voucher.Id);
            Assert.AreEqual(0, afterUndoVoucher.Redeemed);

            var audits = _db.VoucherRedeemAudits.Where(a => a.DeliveryInfoId == info.Id).ToList();
            Assert.AreEqual(2, audits.Count);
            Assert.IsTrue(audits.Any(a => a.Action == "UndoRedeem"));
        }
    }
}