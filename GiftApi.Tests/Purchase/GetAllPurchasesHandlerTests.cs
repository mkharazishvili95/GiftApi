using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.VoucherDeliveryInfo
{
    [TestFixture]
    public class GetAllPurchasesHandlerTests
    {
        private ApplicationDbContext _db;
        private IPurchaseRepository _purchaseRepository;
        private GetAllPurchasesHandler _handler;

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
                Title = "Test Voucher",
                Description = "Voucher Description",
                Amount = 100,
                IsPercentage = false,
                Brand = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "Test Brand" },
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                IsActive = true,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            _purchaseRepository = new PurchaseRepository(_db);
            _handler = new GetAllPurchasesHandler(_purchaseRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_ReturnEmptyList_When_NoPurchasesExist()
        {
            var query = new GetAllPurchasesQuery();

            var result = await _handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.TotalCount);
            Assert.IsEmpty(result.Items);
            Assert.AreEqual("No purchases found", result.Message);
        }

        [Test]
        public async Task Should_ReturnAllPurchases_When_Exist()
        {
            var voucherId = _db.Vouchers.First().Id;

            var purchases = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderName = "Alice",
                    RecipientName = "John Doe",
                    RecipientEmail = "john@example.com",
                    RecipientPhone = "555111222",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Rustaveli 1",
                    Message = "Happy Birthday",
                    Quantity = 1,
                    IsUsed = false
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderName = "Bob",
                    RecipientName = "Jane Smith",
                    RecipientEmail = "jane@example.com",
                    RecipientPhone = "555333444",
                    RecipientCity = "Batumi",
                    RecipientAddress = "Beach St 2",
                    Message = "Congrats",
                    Quantity = 2,
                    IsUsed = true
                }
            };

            _db.VoucherDeliveryInfos.AddRange(purchases);
            await _db.SaveChangesAsync();

            var query = new GetAllPurchasesQuery
            {
                Pagination = new Application.Common.Models.PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);
            Assert.That(result.Items.Any(x => x.RecipientName == "John Doe"));
            Assert.That(result.Items.Any(x => x.RecipientName == "Jane Smith"));
        }

        [Test]
        public async Task Should_Filter_By_IsUsed_When_True()
        {
            var voucherId = _db.Vouchers.First().Id;

            var purchases = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    RecipientName = "Used 1",
                    RecipientPhone = "555-111-111",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Address 1",
                    IsUsed = true
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    RecipientName = "Unused 1",
                    RecipientPhone = "555-222-222",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Address 2",
                    IsUsed = false
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    RecipientName = "Used 2",
                    RecipientPhone = "555-333-333",
                    RecipientCity = "Batumi",
                    RecipientAddress = "Address 3",
                    IsUsed = true
                },
            };

            _db.VoucherDeliveryInfos.AddRange(purchases);
            await _db.SaveChangesAsync();

            var query = new GetAllPurchasesQuery { IsUsed = true };

            var result = await _handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);
            Assert.IsTrue(result.Items.All(x => x.IsUsed == true));
        }

        [Test]
        public async Task Should_ApplyPagination_Correctly()
        {
            var voucherId = _db.Vouchers.First().Id;

            for (int i = 1; i <= 25; i++)
            {
                _db.VoucherDeliveryInfos.Add(new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    RecipientName = $"Recipient {i}",
                    RecipientPhone = "555-000-000",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = $"Address {i}",
                    IsUsed = false
                });
            }
            await _db.SaveChangesAsync();

            var query = new GetAllPurchasesQuery
            {
                Pagination = new Application.Common.Models.PaginationModel
                {
                    Page = 2,
                    PageSize = 10
                }
            };

            var result = await _handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(25, result.TotalCount);
            Assert.AreEqual(10, result.Items.Count);
            Assert.AreEqual("Recipient 11", result.Items.First().RecipientName);
        }

        [Test]
        public async Task Should_Filter_By_SearchString()
        {
            var voucherId = _db.Vouchers.First().Id;

            var purchases = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderName = "Alice",
                    RecipientName = "John Doe",
                    RecipientEmail = "john@example.com",
                    RecipientPhone = "555111222",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Rustaveli 1",
                    Message = "Happy Birthday",
                    Quantity = 1,
                    IsUsed = false
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderName = "Bob",
                    RecipientName = "Jane Smith",
                    RecipientEmail = "jane@example.com",
                    RecipientPhone = "555333444",
                    RecipientCity = "Batumi",
                    RecipientAddress = "Beach St 2",
                    Message = "Congrats",
                    Quantity = 2,
                    IsUsed = true
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderName = "Charlie",
                    RecipientName = "Jack Brown",
                    RecipientEmail = "jack@example.com",
                    RecipientPhone = "555555555",
                    RecipientCity = "Kutaisi",
                    RecipientAddress = "Center 3",
                    Message = "Gift",
                    Quantity = 1,
                    IsUsed = false
                }
            };
            _db.VoucherDeliveryInfos.AddRange(purchases);
            await _db.SaveChangesAsync();

            var query1 = new GetAllPurchasesQuery { SearchString = "John" };
            var result1 = await _handler.Handle(query1, default);
            Assert.AreEqual(1, result1.Items.Count);
            Assert.IsTrue(result1.Items.All(x => x.RecipientName.Contains("John")));

            var query2 = new GetAllPurchasesQuery { SearchString = "jane@example.com" };
            var result2 = await _handler.Handle(query2, default);
            Assert.AreEqual(1, result2.Items.Count);
            Assert.IsTrue(result2.Items.All(x => x.RecipientEmail == "jane@example.com"));

            var query3 = new GetAllPurchasesQuery { SearchString = "Charlie" };
            var result3 = await _handler.Handle(query3, default);
            Assert.AreEqual(1, result3.Items.Count);
            Assert.IsTrue(result3.Items.All(x => x.SenderName == "Charlie"));

            var query4 = new GetAllPurchasesQuery { SearchString = "NonExisting" };
            var result4 = await _handler.Handle(query4, default);
            Assert.AreEqual(0, result4.Items.Count);
            Assert.IsTrue(result4.Success);
            Assert.AreEqual("No purchases found", result4.Message);
        }
        [Test]
        public async Task Should_Filter_By_SenderId()
        {
            var voucherId = _db.Vouchers.First().Id;
            var senderId1 = Guid.NewGuid();
            var senderId2 = Guid.NewGuid();

            var purchases = new List<GiftApi.Domain.Entities.VoucherDeliveryInfo>
            {
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderId = senderId1,
                    SenderName = "Alice",
                    RecipientName = "John Doe",
                    RecipientEmail = "john@example.com",
                    RecipientPhone = "555111222",
                    RecipientCity = "Tbilisi",
                    RecipientAddress = "Rustaveli 1",
                    Message = "Happy Birthday",
                    Quantity = 1,
                    IsUsed = false
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderId = senderId2,
                    SenderName = "Bob",
                    RecipientName = "Jane Smith",
                    RecipientEmail = "jane@example.com",
                    RecipientPhone = "555333444",
                    RecipientCity = "Batumi",
                    RecipientAddress = "Beach St 2",
                    Message = "Congrats",
                    Quantity = 2,
                    IsUsed = true
                },
                new GiftApi.Domain.Entities.VoucherDeliveryInfo
                {
                    Id = Guid.NewGuid(),
                    VoucherId = voucherId,
                    SenderId = null,
                    SenderName = "Charlie",
                    RecipientName = "Jack Brown",
                    RecipientEmail = "jack@example.com",
                    RecipientPhone = "555555555",
                    RecipientCity = "Kutaisi",
                    RecipientAddress = "Center 3",
                    Message = "Gift",
                    Quantity = 1,
                    IsUsed = false
                }
            };

            _db.VoucherDeliveryInfos.AddRange(purchases);
            await _db.SaveChangesAsync();

            var query1 = new GetAllPurchasesQuery { SenderId = senderId1 };
            var result1 = await _handler.Handle(query1, default);

            Assert.IsTrue(result1.Success);
            Assert.AreEqual(1, result1.Items.Count);
            Assert.IsTrue(result1.Items.All(x => x.SenderId == senderId1));

            var query2 = new GetAllPurchasesQuery { SenderId = senderId2 };
            var result2 = await _handler.Handle(query2, default);

            Assert.IsTrue(result2.Success);
            Assert.AreEqual(1, result2.Items.Count);
            Assert.IsTrue(result2.Items.All(x => x.SenderId == senderId2));

            var query3 = new GetAllPurchasesQuery { SenderId = Guid.NewGuid() };
            var result3 = await _handler.Handle(query3, default);

            Assert.IsTrue(result3.Success);
            Assert.AreEqual(0, result3.Items.Count);
            Assert.AreEqual("No purchases found", result3.Message);
        }

    }
}
