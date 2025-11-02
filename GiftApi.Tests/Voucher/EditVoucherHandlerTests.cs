using GiftApi.Application.Features.Manage.Voucher.Commands.Edit;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class EditVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private IBrandRepository _brandRepository;
        private IUnitOfWork _unitOfWork;
        private EditVoucherHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _brandRepository = new BrandRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new EditVoucherHandler(_voucherRepository, _brandRepository, _unitOfWork);
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
            var command = new EditVoucherCommand
            {
                Id = Guid.NewGuid(),
                Title = "Test",
                Amount = 10
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Brand_Does_Not_Exist()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Old Voucher",
                Description = "Some description",
                Amount = 10,
                IsActive = true
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new EditVoucherCommand
            {
                Id = voucher.Id,
                Title = "Updated Voucher",
                Amount = 20,
                BrandId = 999
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("does not exist"));
        }

        [Test]
        public async Task Should_Edit_Voucher_When_Brand_Exists()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 1, Name = "TestBrand", IsDeleted = false });

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Old Voucher",
                Description = "Old description",
                Amount = 10,
                IsPercentage = false,
                IsActive = true
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new EditVoucherCommand
            {
                Id = voucher.Id,
                Title = "Updated Voucher",
                Description = "Updated description",
                Amount = 25,
                IsPercentage = false,
                BrandId = 1,
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 50,
                Redeemed = 0,
                IsActive = true,
                ImageUrl = "updated.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(voucher.Id, result.Id);

            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == voucher.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.AreEqual("Updated Voucher", voucherInDb.Title);
            Assert.AreEqual("Updated description", voucherInDb.Description);
            Assert.AreEqual(25, voucherInDb.Amount);
            Assert.AreEqual(1, voucherInDb.BrandId);
            Assert.AreEqual("updated.png", voucherInDb.ImageUrl);
        }

        [Test]
        public async Task Should_Edit_Voucher_When_BrandId_Is_Null()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Old Voucher",
                Description = "Some description",
                Amount = 10,
                IsActive = true
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new EditVoucherCommand
            {
                Id = voucher.Id,
                Title = "Edited Voucher",
                Amount = 50,
                BrandId = null
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == voucher.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.IsNull(voucherInDb.BrandId);
            Assert.AreEqual("Edited Voucher", voucherInDb.Title);
            Assert.AreEqual(50, voucherInDb.Amount);
        }

        [Test]
        public async Task Should_Edit_Unlimited_Voucher()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Old Voucher",
                Description = "Some description",
                Amount = 10,
                Unlimited = false
            };
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var command = new EditVoucherCommand
            {
                Id = voucher.Id,
                Title = "Unlimited Voucher",
                Amount = 100,
                Unlimited = true,
                Quantity = 9999
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == voucher.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.IsTrue(voucherInDb.Unlimited);
            Assert.AreEqual(9999, voucherInDb.Quantity);
        }
    }
}
