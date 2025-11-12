using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Export;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace GiftApi.Tests.VoucherDeliveryInfo
{
    [TestFixture]
    public class ExportAllPurchasesHandlerTests
    {
        private ApplicationDbContext _db;
        private IPurchaseRepository _purchaseRepository;
        private ExportAllPurchasesHandler _handler;
        private Guid _voucherId1;
        private Guid _voucherId2;
        private Guid _senderId1;
        private Guid _senderId2;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            var voucher1 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Main Voucher",
                Description = "Desc 1",
                Amount = 50,
                IsPercentage = false,
                Brand = new GiftApi.Domain.Entities.Brand { Id = 11, Name = "Brand A" },
                ValidMonths = 6,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            var voucher2 = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Secondary Voucher",
                Description = "Desc 2",
                Amount = 75,
                IsPercentage = false,
                Brand = new GiftApi.Domain.Entities.Brand { Id = 12, Name = "Brand B" },
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 200,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _voucherId1 = voucher1.Id;
            _voucherId2 = voucher2.Id;
            _senderId1 = Guid.NewGuid();
            _senderId2 = Guid.NewGuid();

            _db.Vouchers.AddRange(voucher1, voucher2);
            await _db.SaveChangesAsync();

            _purchaseRepository = new PurchaseRepository(_db);
            _handler = new ExportAllPurchasesHandler(_purchaseRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        private async Task SeedBasicPurchases()
        {
            var list = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    VoucherId = _voucherId1,
                    SenderId = _senderId1,
                    SenderName = "Alice",
                    RecipientName = "John Doe",
                    RecipientEmail = "john@example.com",
                    RecipientPhone = "555111222",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Addr 1",
                    Message = "Happy Birthday",
                    Quantity = 1,
                    IsUsed = false
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    VoucherId = _voucherId1,
                    SenderId = _senderId2,
                    SenderName = "Bob",
                    RecipientName = "Jane Smith",
                    RecipientEmail = "jane@example.com",
                    RecipientPhone = "555333444",
                    RecipientCity = "Batumi",
                    RecipientAddress = "Addr 2",
                    Message = "Congrats",
                    Quantity = 2,
                    IsUsed = true
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    VoucherId = _voucherId2,
                    SenderId = null,
                    SenderName = "Charlie",
                    RecipientName = "Jack Brown",
                    RecipientEmail = "jack@example.com",
                    RecipientPhone = "555555555",
                    RecipientCity = "Kutaisi",
                    RecipientAddress = "Addr 3",
                    Message = "Gift",
                    Quantity = 3,
                    IsUsed = false
                }
            };
            _db.VoucherDeliveryInfos.AddRange(list);
            await _db.SaveChangesAsync();
        }

        [Test]
        public async Task Should_ReturnEmptyCsv_When_NoPurchases()
        {
            var query = new ExportAllPurchasesQuery();
            var result = await _handler.Handle(query, default);

            Assert.NotNull(result);
            var csv = Encoding.UTF8.GetString(result.Data);
            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(1, lines.Length, "Only header expected");
            StringAssert.StartsWith("Id,VoucherId,VoucherTitle", lines[0]);
        }

        [Test]
        public async Task Should_Export_AllPurchases()
        {
            await SeedBasicPurchases();

            var result = await _handler.Handle(new ExportAllPurchasesQuery(), default);
            var csv = Encoding.UTF8.GetString(result.Data);

            var lines = csv.TrimEnd().Split('\n');
            Assert.AreEqual(4, lines.Length);
            Assert.IsTrue(lines[0].Contains("VoucherTitle"));
            Assert.IsTrue(csv.Contains("Main Voucher"));
            Assert.IsTrue(csv.Contains("Secondary Voucher"));
            Assert.AreEqual("text/csv", result.ContentType);
            StringAssert.StartsWith("purchases-", result.FileName);
            StringAssert.EndsWith(".csv", result.FileName);
        }

        [Test]
        public async Task Should_Filter_By_IsUsed()
        {
            await SeedBasicPurchases();

            var resUsed = await _handler.Handle(new ExportAllPurchasesQuery { IsUsed = true }, default);
            var csvUsed = Encoding.UTF8.GetString(resUsed.Data);
            var linesUsed = csvUsed.TrimEnd().Split('\n');
            Assert.AreEqual(2, linesUsed.Length);
            Assert.IsTrue(csvUsed.Contains("true"));
            Assert.IsFalse(csvUsed.Contains("false,"));

            var resUnused = await _handler.Handle(new ExportAllPurchasesQuery { IsUsed = false }, default);
            var csvUnused = Encoding.UTF8.GetString(resUnused.Data);
            var linesUnused = csvUnused.TrimEnd().Split('\n');
            Assert.AreEqual(3, linesUnused.Length);
            Assert.IsTrue(csvUnused.Contains("false"));
            Assert.IsFalse(csvUnused.Contains(",true"));
        }
    }
}