using GiftApi.Application.Features.Manage.Voucher.Commands.Create;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class CreateVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private IBrandRepository _brandRepository;
        private IUnitOfWork _unitOfWork;
        private CreateVoucherHandler _handler;

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
            _handler = new CreateVoucherHandler(_unitOfWork, _voucherRepository, _brandRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Brand_Does_Not_Exist()
        {
            var command = new CreateVoucherCommand
            {
                Title = "10% OFF",
                Description = "Discount voucher",
                Amount = 10,
                IsPercentage = true,
                BrandId = 999,
                ValidMonths = 6,
                Unlimited = false,
                Quantity = 100,
                Redeemed = 0,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand with Id 999 does not exist"));
        }

        [Test]
        public async Task Should_Create_Voucher_When_Brand_Exists()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 1, Name = "TestBrand", IsDeleted = false });
            await _db.SaveChangesAsync();

            var command = new CreateVoucherCommand
            {
                Title = "20 GEL Gift Card",
                Description = "Voucher for shopping",
                Amount = 20,
                IsPercentage = false,
                BrandId = 1,
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 50,
                Redeemed = 0,
                IsActive = true,
                ImageUrl = "test.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("successfully"));
            Assert.AreNotEqual(Guid.Empty, result.Id);

            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == result.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.AreEqual("20 GEL Gift Card", voucherInDb.Title);
            Assert.AreEqual(20, voucherInDb.Amount);
            Assert.AreEqual(1, voucherInDb.BrandId);
            Assert.AreEqual("test.png", voucherInDb.ImageUrl);
        }

        [Test]
        public async Task Should_Create_Voucher_When_No_BrandId_Provided()
        {
            var command = new CreateVoucherCommand
            {
                Title = "No Brand Voucher",
                Description = "Voucher without brand",
                Amount = 50,
                IsPercentage = false,
                BrandId = null,
                ValidMonths = 6,
                Unlimited = true,
                Quantity = 0,
                Redeemed = 0,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);

            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == result.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.IsNull(voucherInDb.BrandId);
            Assert.IsTrue(voucherInDb.Unlimited);
        }

        [Test]
        public async Task Should_Create_Voucher_With_Negative_Amount()
        {
            _db.Brands.Add(new GiftApi.Domain.Entities.Brand { Id = 2, Name = "TestBrand2", IsDeleted = false });
            await _db.SaveChangesAsync();

            var command = new CreateVoucherCommand
            {
                Title = "Negative Amount",
                Description = "Voucher with negative amount",
                Amount = -10,
                IsPercentage = false,
                BrandId = 2,
                ValidMonths = 3,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(-10, result.Amount);

            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == result.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.AreEqual(-10, voucherInDb.Amount);
        }

        [Test]
        public async Task Should_Create_Unlimited_Voucher_Ignoring_Quantity()
        {
            var command = new CreateVoucherCommand
            {
                Title = "Unlimited Voucher",
                Description = "Unlimited quantity",
                Amount = 100,
                IsPercentage = false,
                BrandId = null,
                ValidMonths = 12,
                Unlimited = true,
                Quantity = 9999,
                Redeemed = 0,
                IsActive = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var voucherInDb = await _db.Vouchers.FirstOrDefaultAsync(v => v.Id == result.Id);
            Assert.IsNotNull(voucherInDb);
            Assert.IsTrue(voucherInDb.Unlimited);
            Assert.AreEqual(9999, voucherInDb.Quantity);
        }
    }
}
