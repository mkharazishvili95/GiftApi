using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Voucher.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class GetAllVouchersHandlerTests
    {
        private ApplicationDbContext _db;
        private IVoucherRepository _voucherRepository;
        private GetAllVouchersHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _voucherRepository = new VoucherRepository(_db);
            _handler = new GetAllVouchersHandler(_voucherRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_EmptyList_When_NoVouchersExist()
        {
            var query = new GetAllVouchersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, result.TotalCount);
            Assert.IsEmpty(result.Items);
        }

        [Test]
        public async Task Should_Return_Voucher_With_NullBrand_When_Brand_IsDeleted()
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
                Description = "Description",
                Amount = 100,
                ValidMonths = 12,
                IsPercentage = false,
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

            var query = new GetAllVouchersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.TotalCount);

            var item = result.Items.First();
            Assert.AreEqual(voucher.Title, item.Title);
            Assert.IsNull(item.Brand);
            Assert.IsNull(item.Category);
        }

        [Test]
        public async Task Should_Return_Voucher_With_NullCategory_When_Category_IsDeleted()
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
                Amount = 100,
                ValidMonths = 12,
                IsPercentage = false,
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

            var query = new GetAllVouchersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            var item = result.Items.First();

            Assert.IsNotNull(item.Brand);
            Assert.IsNull(item.Category);
        }

        [Test]
        public async Task Should_Return_Voucher_With_Brand_And_Category_When_AllActive()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Id = 1,
                Name = "Active Category",
                Description = "Description",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Id = 1,
                Name = "Active Brand",
                Description = "Description",
                IsDeleted = false,
                CategoryId = category.Id,
                Category = category,
                CreateDate = DateTime.UtcNow
            };

            var voucher = new GiftApi.Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "Voucher Active",
                Description = "Description",
                Amount = 150,
                ValidMonths = 6,
                IsPercentage = false,
                Unlimited = false,
                Quantity = 5,
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

            var query = new GetAllVouchersQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.TotalCount);
            var item = result.Items.First();

            Assert.IsNotNull(item.Brand);
            Assert.IsNotNull(item.Category);
            Assert.AreEqual(voucher.Title, item.Title);
        }

        [Test]
        public async Task Should_Apply_Pagination_Correctly()
        {
            for (int i = 1; i <= 25; i++)
            {
                _db.Vouchers.Add(new GiftApi.Domain.Entities.Voucher
                {
                    Id = Guid.NewGuid(),
                    Title = $"Voucher {i}",
                    Description = "Description",
                    Amount = 50,
                    IsDeleted = false,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            var query = new GetAllVouchersQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(25, result.TotalCount);
            Assert.AreEqual(10, result.Items.Count);
        }
    }
}