using GiftApi.Application.Features.Voucher.Queries.Get;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class GetVoucherHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private GetVoucherHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _handler = new GetVoucherHandler(_voucherRepository);
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
            var query = new GetVoucherQuery { Id = Guid.Empty };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Id is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Voucher_Does_Not_Exist()
        {
            var query = new GetVoucherQuery { Id = Guid.NewGuid() };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Voucher with Id"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Voucher_IsDeleted()
        {
            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Test Voucher",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow,
                Description = "Description", 
            };

            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var query = new GetVoucherQuery { Id = voucher.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("is deleted"));
        }

        [Test]
        public async Task Should_Return_Voucher_With_NullBrand_When_BrandIsDeleted()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Id = 1,
                Name = "Category 1",
                Description = "Category description",
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Id = 1,
                Name = "Brand 1",
                Description = "Brand description",
                IsDeleted = true,
                CategoryId = category.Id,
                Category = category,
                CreateDate = DateTime.UtcNow
            };

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 1",
                Description = "Voucher description", 
                Amount = 100,                        
                IsPercentage = false,                
                ValidMonths = 12,                     
                Unlimited = false,                    
                Quantity = 10,                       
                Redeemed = 0,                         
                IsActive = true,                      
                IsDeleted = false,
                BrandId = brand.Id,
                Brand = brand,
                CreateDate = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            _db.Brands.Add(brand);
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync(); 

            var query = new GetVoucherQuery { Id = voucher.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.IsNull(result.Brand);   
            Assert.IsNull(result.Category); 
        }


        [Test]
        public async Task Should_Return_Voucher_With_NullCategory_When_CategoryIsDeleted()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Id = 1,
                Name = "Category 1",
                Description = "Description",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow
            };

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Id = 1,
                Name = "Brand 1",
                Description = "Description",
                IsDeleted = false,
                CategoryId = category.Id,
                Category = category,
                CreateDate = DateTime.UtcNow
            };

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 1",
                Description = "Description",
                IsDeleted = false,
                IsActive = true,
                BrandId = brand.Id,
                Brand = brand,
                CreateDate = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            _db.Brands.Add(brand);
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var query = new GetVoucherQuery { Id = voucher.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Brand);
            Assert.IsNull(result.Category);
        }

        [Test]
        public async Task Should_Return_Voucher_With_Brand_And_Category_When_AllActive()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Id = 1,
                Name = "Category 1",
                Description = "Description",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Id = 1,
                Name = "Brand 1",
                IsDeleted = false,
                CategoryId = category.Id,
                Category = category,
                CreateDate = DateTime.UtcNow
            };

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher 1",
                Description = "Description",
                Amount = 100,
                IsPercentage = false,
                ValidMonths = 12,
                Unlimited = false,
                Quantity = 10,
                Redeemed = 0,
                IsActive = true,
                IsDeleted = false,
                BrandId = brand.Id,
                Brand = brand,
                CreateDate = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            _db.Brands.Add(brand);
            _db.Vouchers.Add(voucher);
            await _db.SaveChangesAsync();

            var query = new GetVoucherQuery { Id = voucher.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.Brand);
            Assert.IsNotNull(result.Category);
            Assert.AreEqual(voucher.Title, result.Title);
            Assert.AreEqual(voucher.Amount, result.Amount);
        }
    }
}