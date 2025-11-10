using GiftApi.Application.Features.Manage.VoucherDeliveryInfo.Commands.ChangeStatus;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GiftApi.Tests.Manage
{
    [TestFixture]
    public class ChangeStatusHandlerTests
    {
        private ApplicationDbContext _db;
        private ManageRepository _repository;
        private ChangeStatusHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _db = new ApplicationDbContext(options);
            _repository = new ManageRepository(_db);
            _handler = new ChangeStatusHandler(_repository);
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }

        [Test]
        public async Task Should_ReturnBadRequest_When_Id_IsEmpty()
        {
            var command = new ChangeStatusCommand { DeliveryInfoId = Guid.Empty };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Id is required"));
        }

        [Test]
        public async Task Should_ReturnNotFound_When_DeliveryInfo_NotExists()
        {
            var command = new ChangeStatusCommand { DeliveryInfoId = Guid.NewGuid() };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_ReturnNotFound_When_DeliveryInfo_AlreadyUsed()
        {
            var deliveryInfo = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = Guid.NewGuid(),
                RecipientName = "John",
                RecipientPhone = "599123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1",
                IsUsed = true
            };

            _db.VoucherDeliveryInfos.Add(deliveryInfo);
            await _db.SaveChangesAsync();

            var command = new ChangeStatusCommand { DeliveryInfoId = deliveryInfo.Id };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("already used"));
        }

        [Test]
        public async Task Should_ReturnSuccess_When_Status_IsChanged()
        {
            var deliveryInfo = new GiftApi.Domain.Entities.VoucherDeliveryInfo
            {
                Id = Guid.NewGuid(),
                VoucherId = Guid.NewGuid(),
                RecipientName = "John",
                RecipientPhone = "599123456",
                RecipientCity = "Tbilisi",
                RecipientAddress = "Street 1",
                IsUsed = false
            };

            _db.VoucherDeliveryInfos.Add(deliveryInfo);
            await _db.SaveChangesAsync();

            var command = new ChangeStatusCommand { DeliveryInfoId = deliveryInfo.Id };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("status changed"));

            var updatedInfo = await _db.VoucherDeliveryInfos.FindAsync(deliveryInfo.Id);
            Assert.IsTrue(updatedInfo.IsUsed);
        }
    }
}
