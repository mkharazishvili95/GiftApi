using GiftApi.Application.Features.Manage.Voucher.Commands.Restore;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class RestoreVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private IUnitOfWork _unitOfWork;
        private RestoreVoucherHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new RestoreVoucherHandler(_voucherRepository, _unitOfWork);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_NotFound_When_Voucher_Does_Not_Exist()
        {
            var command = new RestoreVoucherCommand { Id = Guid.NewGuid() };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Voucher_Is_Not_Deleted()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "ActiveVoucher",
                Description = "Desc",
                Amount = 15,
                IsActive = true,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new RestoreVoucherCommand { Id = voucher.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not deleted"));
        }

        [Test]
        public async Task Should_Restore_Voucher_Successfully()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "DeletedVoucher",
                Description = "Desc",
                Amount = 50,
                IsActive = false,
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow.AddMinutes(-30),
                CreateDate = DateTime.UtcNow.AddHours(-2)
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            _db.Entry(voucher).State = EntityState.Detached;

            var command = new RestoreVoucherCommand { Id = voucher.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("restored successfully"));

            var restored = await _db.Vouchers.FirstAsync(v => v.Id == voucher.Id);
            Assert.IsFalse(restored.IsDeleted);
            Assert.IsNull(restored.DeleteDate);
            Assert.IsFalse(restored.IsActive);
            Assert.IsNotNull(restored.UpdateDate);
        }
    }
}