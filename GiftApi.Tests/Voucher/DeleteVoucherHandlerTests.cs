using GiftApi.Application.Features.Manage.Voucher.Commands.Delete;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class DeleteVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private IUnitOfWork _unitOfWork;
        private DeleteVoucherHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new DeleteVoucherHandler(_voucherRepository, _unitOfWork);
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
            var command = new DeleteVoucherCommand { Id = Guid.NewGuid() };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Voucher_Already_Deleted()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "DeletedVoucher",
                Description = "Desc",
                Amount = 10,
                IsActive = false,
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow.AddMinutes(-10),
                CreateDate = DateTime.UtcNow.AddHours(-1)
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new DeleteVoucherCommand { Id = voucher.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("already deleted"));
        }

        [Test]
        public async Task Should_Delete_Voucher_Successfully()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "ActiveVoucher",
                Description = "Desc",
                Amount = 25,
                IsActive = true,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new DeleteVoucherCommand { Id = voucher.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("deleted successfully"));

            var voucherInDb = await _db.Vouchers.FirstAsync(v => v.Id == voucher.Id);
            Assert.IsTrue(voucherInDb.IsDeleted);
            Assert.IsFalse(voucherInDb.IsActive);
            Assert.IsNotNull(voucherInDb.DeleteDate);
            Assert.IsNotNull(voucherInDb.UpdateDate);
        }
    }
}