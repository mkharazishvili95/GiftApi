using FluentAssertions;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class PurchaseRepositoryTests
    {
        ApplicationDbContext _db = null!;
        PurchaseRepository _repo = null!;
        Guid _voucherId;
        Guid _deliveryIdUsed;
        Guid _deliveryIdUnused;
        Guid _userId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new PurchaseRepository(_db);

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand B",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher",
                Description = "Desc",
                Amount = 30,
                IsPercentage = false,
                BrandId = brand.Id,
                ValidMonths = 1,
                Unlimited = false,
                Quantity = 5,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow.AddDays(-1),
                IsDeleted = false,
                SoldCount = 5
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();
            _voucherId = voucher.Id;

            var usedDelivery = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                Voucher = voucher,
                RecipientName = "Used",
                RecipientPhone = "111",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 2,
                IsUsed = true,
                UsedDate = DateTime.UtcNow.AddHours(-1),
                CreateDate = DateTime.UtcNow.AddHours(-2)
            };
            var unusedDelivery = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = voucher.Id,
                Voucher = voucher,
                RecipientName = "Unused",
                RecipientPhone = "222",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 1,
                IsUsed = false,
                CreateDate = DateTime.UtcNow.AddHours(-3)
            };
            _db.VoucherDeliveryInfos.AddRange(usedDelivery, unusedDelivery);
            await _db.SaveChangesAsync();
            _deliveryIdUsed = usedDelivery.Id;
            _deliveryIdUnused = unusedDelivery.Id;

            _userId = Guid.NewGuid();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Get_Should_Return_Entity()
        {
            var entity = await _repo.Get(_deliveryIdUsed);
            entity.Should().NotBeNull();
            entity!.RecipientName.Should().Be("Used");
        }

        [Test]
        public async Task Get_Should_Return_Null_For_Missing()
        {
            (await _repo.Get(Guid.NewGuid())).Should().BeNull();
        }

        [Test]
        public async Task GetAll_Should_Return_List()
        {
            var list = await _repo.GetAll();
            list.Should().NotBeNull();
            list!.Count.Should().Be(2);
            list.All(x => x.Voucher != null).Should().BeTrue();
        }

        [Test]
        public async Task RedeemAsync_Should_Apply_Redeem()
        {
            var ok = await _repo.RedeemAsync(_deliveryIdUnused, _userId);
            ok.Should().BeTrue();

            var delivery = await _db.VoucherDeliveryInfos.FindAsync(_deliveryIdUnused);
            delivery!.IsUsed.Should().BeTrue();
            delivery.UsedDate.Should().NotBeNull();

            var voucher = await _db.Vouchers.FindAsync(_voucherId);
            voucher!.Redeemed.Should().Be(1);
            (await _db.VoucherRedeemAudits.CountAsync()).Should().Be(1);
            var audit = await _db.VoucherRedeemAudits.FirstAsync();
            audit.Action.Should().Be("Redeem");
            audit.Quantity.Should().Be(1);
            audit.PreviousIsUsed.Should().BeFalse();
            audit.NewIsUsed.Should().BeTrue();
        }

        [Test]
        public async Task RedeemAsync_Should_Return_False_When_Already_Used()
        {
            (await _repo.RedeemAsync(_deliveryIdUsed, _userId)).Should().BeFalse();
        }

        [Test]
        public async Task RedeemAsync_Should_Return_False_When_Expired()
        {
            var voucher = await _db.Vouchers.FindAsync(_voucherId);
            voucher!.CreateDate = DateTime.UtcNow.AddMonths(-2);
            voucher.ValidMonths = 1;
            _db.Vouchers.Update(voucher);
            await _db.SaveChangesAsync();

            var id = Guid.NewGuid();
            var delivery = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = id,
                VoucherId = _voucherId,
                Voucher = voucher,
                RecipientName = "Exp",
                RecipientPhone = "333",
                RecipientCity = "City",
                RecipientAddress = "Addr",
                Quantity = 1,
                IsUsed = false,
                CreateDate = DateTime.UtcNow
            };
            _db.VoucherDeliveryInfos.Add(delivery);
            await _db.SaveChangesAsync();

            (await _repo.RedeemAsync(id, _userId)).Should().BeFalse();
        }

        [Test]
        public async Task UndoRedeemAsync_Should_Revert()
        {
            var ok = await _repo.UndoRedeemAsync(_deliveryIdUsed, _userId);
            ok.Should().BeTrue();

            var delivery = await _db.VoucherDeliveryInfos.FindAsync(_deliveryIdUsed);
            delivery!.IsUsed.Should().BeFalse();
            delivery.UsedDate.Should().BeNull();

            var voucher = await _db.Vouchers.FindAsync(_voucherId);
            voucher!.Redeemed.Should().Be(0);
            (await _db.VoucherRedeemAudits.CountAsync()).Should().Be(1);
            var audit = await _db.VoucherRedeemAudits.FirstAsync();
            audit.Action.Should().Be("UndoRedeem");
            audit.Quantity.Should().Be(2);
            audit.PreviousIsUsed.Should().BeTrue();
            audit.NewIsUsed.Should().BeFalse();
        }

        [Test]
        public async Task UndoRedeemAsync_Should_Return_False_When_Not_Used()
        {
            (await _repo.UndoRedeemAsync(_deliveryIdUnused, _userId)).Should().BeFalse();
        }

        [Test]
        public async Task GetVoucherRedeemAuditsAsync_Should_Return_Audits()
        {
            await _repo.RedeemAsync(_deliveryIdUnused, _userId);
            var audits = await _repo.GetVoucherRedeemAuditsAsync(_voucherId);
            audits.Should().NotBeNull();
            audits.Count.Should().Be(1);
            audits.First().VoucherId.Should().Be(_voucherId);
        }
    }
}