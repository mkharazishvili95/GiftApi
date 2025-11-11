using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Queries.Get;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Purchase
{
    [TestFixture]
    public class GetPurchaseHandlerTests
    {
        private ApplicationDbContext _db;
        private IPurchaseRepository _purchaseRepository;
        private GetPurchaseHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _purchaseRepository = new PurchaseRepository(_db);
            _handler = new GetPurchaseHandler(_purchaseRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Id_Is_Empty()
        {
            var query = new GetPurchaseQuery { Id = Guid.Empty };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Id is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Purchase_Does_Not_Exist()
        {
            var query = new GetPurchaseQuery { Id = Guid.NewGuid() };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_Purchase_Successfully()
        {
            var purchase = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = Guid.NewGuid(),
                SenderName = "Misho",
                RecipientName = "Nino",
                RecipientEmail = "nino@example.com",
                RecipientPhone = "555111222",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Rustaveli Ave",
                Message = "Happy Birthday",
                SenderId = Guid.NewGuid(),
                Quantity = 3,
                IsUsed = false
            };

            _db.VoucherDeliveryInfos.Add(purchase);
            await _db.SaveChangesAsync();

            var query = new GetPurchaseQuery { Id = purchase.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(purchase.Id, result.Id);
            Assert.AreEqual(purchase.VoucherId, result.VoucherId);
            Assert.AreEqual("Misho", result.SenderName);
            Assert.AreEqual("Nino", result.RecipientName);
            Assert.AreEqual("nino@example.com", result.RecipientEmail);
            Assert.AreEqual("555111222", result.RecipientPhone);
            Assert.AreEqual("Tbilisi", result.RecipientCity);
            Assert.AreEqual("Rustaveli Ave", result.RecipientAddress);
            Assert.AreEqual(purchase.Quantity, result.Quantity);
            Assert.AreEqual(purchase.IsUsed, result.IsUsed);
        }
    }
}
