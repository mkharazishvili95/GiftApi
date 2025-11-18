using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.RedeemAudits;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Purchase
{
    [TestFixture]
    public class GetVoucherRedeemAuditsHandlerTests
    {
        private ApplicationDbContext _db = null!;
        private IPurchaseRepository _repo = null!;
        private GetVoucherRedeemAuditsHandler _handler = null!;
        private Guid _voucherId = Guid.Empty;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            _db = new ApplicationDbContext(options);

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Audit Voucher",
                Description = "Desc",
                Amount = 10,
                IsPercentage = false,
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 50,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false,
                Brand = new GiftApi.Domain.Entities.Brand { Id = 9, Name = "Brand" }
            };
            _voucherId = voucher.Id;
            _db.Vouchers.Add(voucher);

            await _db.SaveChangesAsync();

            _repo = new PurchaseRepository(_db);
            _handler = new GetVoucherRedeemAuditsHandler(_repo);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task<Guid> CreateDeliveryInfoAsync(int quantity = 1)
        {
            var info = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = _voucherId,
                RecipientName = "User",
                RecipientPhone = "555",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = quantity,
                IsUsed = false
            };
            _db.VoucherDeliveryInfos.Add(info);
            await _db.SaveChangesAsync();
            return info.Id;
        }

        [Test]
        public async Task Should_Return_Error_When_VoucherId_Empty()
        {
            var query = new GetVoucherRedeemAuditsQuery { VoucherId = Guid.Empty };
            var res = await _handler.Handle(query, default);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(400, res.StatusCode);
            Assert.AreEqual("VoucherId is required.", res.Message);
        }

        [Test]
        public async Task Should_Return_Empty_When_No_Audits()
        {
            var query = new GetVoucherRedeemAuditsQuery { VoucherId = _voucherId };
            var res = await _handler.Handle(query, default);
            Assert.IsTrue(res.Success);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual(0, res.Items.Count);
        }

        [Test]
        public async Task Should_Return_Audits_In_Descending_Order()
        {
            var d1 = await CreateDeliveryInfoAsync(2);
            var d2 = await CreateDeliveryInfoAsync(1);

            await _repo.RedeemAsync(d1, Guid.NewGuid());
            await Task.Delay(10);
            await _repo.RedeemAsync(d2, Guid.NewGuid());
            await Task.Delay(10);
            await _repo.UndoRedeemAsync(d1, Guid.NewGuid());

            var query = new GetVoucherRedeemAuditsQuery { VoucherId = _voucherId };
            var res = await _handler.Handle(query, default);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual(3, res.Items.Count);

            var ordered = res.Items.Select(i => i.PerformedAt).ToList();
            var sorted = ordered.OrderByDescending(x => x).ToList();
            CollectionAssert.AreEqual(sorted, ordered);
        }

        [Test]
        public async Task Should_Respect_Take_Limit()
        {
            var d1 = await CreateDeliveryInfoAsync();
            var d2 = await CreateDeliveryInfoAsync();
            var d3 = await CreateDeliveryInfoAsync();

            await _repo.RedeemAsync(d1, Guid.NewGuid());
            await _repo.RedeemAsync(d2, Guid.NewGuid());
            await _repo.RedeemAsync(d3, Guid.NewGuid());

            var query = new GetVoucherRedeemAuditsQuery { VoucherId = _voucherId, Take = 2 };
            var res = await _handler.Handle(query, default);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(2, res.Items.Count);
        }

        [Test]
        public async Task Should_Map_Audit_Fields_Correctly()
        {
            var d1 = await CreateDeliveryInfoAsync(quantity: 5);

            var performer1 = Guid.NewGuid();
            var performer2 = Guid.NewGuid();

            var redeemOk = await _repo.RedeemAsync(d1, performer1);
            Assert.IsTrue(redeemOk);

            var undoOk = await _repo.UndoRedeemAsync(d1, performer2);
            Assert.IsTrue(undoOk);

            var query = new GetVoucherRedeemAuditsQuery { VoucherId = _voucherId };
            var res = await _handler.Handle(query, default);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual(2, res.Items.Count);

            var redeemAudit = res.Items.Single(x => x.Action == "Redeem");
            Assert.AreEqual(d1, redeemAudit.DeliveryInfoId);
            Assert.AreEqual(5, redeemAudit.Quantity);
            Assert.IsFalse(redeemAudit.PreviousIsUsed);
            Assert.IsTrue(redeemAudit.NewIsUsed);

            var undoAudit = res.Items.Single(x => x.Action == "UndoRedeem");
            Assert.AreEqual(d1, undoAudit.DeliveryInfoId);
            Assert.AreEqual(5, undoAudit.Quantity);
            Assert.IsTrue(undoAudit.PreviousIsUsed);
            Assert.IsFalse(undoAudit.NewIsUsed);
        }
    }
}