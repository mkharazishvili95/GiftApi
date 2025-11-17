using GiftApi.Application.Features.Manage.Voucher.Commands.Activate;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class ActivateVoucherHandlerTests
    {
        ApplicationDbContext _db;
        IVoucherRepository _repo;
        IUnitOfWork _uow;
        ActivateVoucherHandler _handler;

        [SetUp]
        public void Setup()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _db = new ApplicationDbContext(opts);
            _repo = new VoucherRepository(_db);
            _uow = new UnitOfWork(_db);
            _handler = new ActivateVoucherHandler(_repo, _uow);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_NotFound()
        {
            var cmd = new ActivateVoucherCommand { Id = Guid.NewGuid(), IsActive = true };
            var res = await _handler.Handle(cmd, CancellationToken.None);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(404, res.StatusCode);
        }

        [Test]
        public async Task Should_Activate_Voucher()
        {
            var v = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Desc",
                Amount = 10,
                IsActive = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(v);
            await _db.SaveChangesAsync();

            var cmd = new ActivateVoucherCommand { Id = v.Id, IsActive = true };
            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(200, res.StatusCode);
            Assert.IsTrue(res.IsActive);
        }

        [Test]
        public async Task Should_Deactivate_Voucher()
        {
            var v = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Description = "Desc",
                Amount = 10,
                IsActive = true,
                CreateDate = DateTime.UtcNow
            };
            _db.Vouchers.Add(v);
            await _db.SaveChangesAsync();

            var cmd = new ActivateVoucherCommand { Id = v.Id, IsActive = false };
            var res = await _handler.Handle(cmd, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.IsFalse(res.IsActive);
        }
    }
}