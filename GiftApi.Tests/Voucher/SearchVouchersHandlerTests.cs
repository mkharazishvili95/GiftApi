using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Voucher.Queries.Search;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Voucher
{
    [TestFixture]
    public class SearchVouchersHandlerTests
    {
        ApplicationDbContext _db;
        VoucherRepository _repo;
        SearchVouchersHandler _handler;

        [SetUp]
        public void Setup()
        {
            var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(opts);
            _repo = new VoucherRepository(_db);
            _handler = new SearchVouchersHandler(_repo);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task EmptyDatabase_Should_Return_Empty_Success()
        {
            var query = new SearchVouchersQuery { Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var res = await _handler.Handle(query, CancellationToken.None);
            Assert.IsTrue(res.Success);
            Assert.AreEqual(200, res.StatusCode);
            Assert.AreEqual(0, res.TotalCount);
            Assert.AreEqual(0, res.Items.Count);
        }

        [Test]
        public async Task MinGreaterThanMax_Should_Return_400()
        {
            var query = new SearchVouchersQuery { MinAmount = 50, MaxAmount = 10 };
            var res = await _handler.Handle(query, CancellationToken.None);
            Assert.IsFalse(res.Success);
            Assert.AreEqual(400, res.StatusCode);
            Assert.That(res.Message, Does.Contain("minAmount"));
        }

        [Test]
        public async Task Should_Filter_By_BrandId()
        {
            var brand1 = new GiftApi.Domain.Entities.Brand { Id = 1, Name = "Brand1", IsDeleted = false };
            var brand2 = new GiftApi.Domain.Entities.Brand { Id = 2, Name = "Brand2", IsDeleted = false };
            _db.Brands.AddRange(brand1, brand2);

            _db.Vouchers.AddRange(
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "A", Description = "D", Amount = 10, IsPercentage = false, BrandId = 1, Brand = brand1, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "B", Description = "D", Amount = 20, IsPercentage = false, BrandId = 2, Brand = brand2, IsActive = true, CreateDate = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var query = new SearchVouchersQuery { BrandId = 2 };
            var res = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.AreEqual(2, res.Items.First().BrandId);
            Assert.AreEqual("B", res.Items.First().Title);
        }

        [Test]
        public async Task Should_Filter_By_CategoryId()
        {
            var cat1 = new GiftApi.Domain.Entities.Category { Id = 10, Name = "Cat10", IsDeleted = false, CreateDate = DateTime.UtcNow };
            var cat2 = new GiftApi.Domain.Entities.Category { Id = 11, Name = "Cat11", IsDeleted = false, CreateDate = DateTime.UtcNow };
            var brandA = new GiftApi.Domain.Entities.Brand { Id = 5, Name = "BrandA", CategoryId = cat1.Id, Category = cat1, IsDeleted = false };
            var brandB = new GiftApi.Domain.Entities.Brand { Id = 6, Name = "BrandB", CategoryId = cat2.Id, Category = cat2, IsDeleted = false };
            _db.Categories.AddRange(cat1, cat2);
            _db.Brands.AddRange(brandA, brandB);

            _db.Vouchers.AddRange(
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "V1", Description = "D", Amount = 30, BrandId = brandA.Id, Brand = brandA, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "V2", Description = "D", Amount = 40, BrandId = brandB.Id, Brand = brandB, IsActive = true, CreateDate = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var query = new SearchVouchersQuery { CategoryId = cat2.Id };
            var res = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.AreEqual("V2", res.Items.Single().Title);
            Assert.AreEqual(cat2.Id, res.Items.Single().CategoryId);
        }

        [Test]
        public async Task Should_Filter_By_Amount_Range()
        {
            _db.Vouchers.AddRange(
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Low", Description = "D", Amount = 5, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Mid", Description = "D", Amount = 25, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "High", Description = "D", Amount = 100, IsActive = true, CreateDate = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var query = new SearchVouchersQuery { MinAmount = 10, MaxAmount = 50 };
            var res = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.AreEqual("Mid", res.Items.Single().Title);
        }

        [Test]
        public async Task Term_Should_Search_Title_Description_Brand_Category()
        {
            var cat = new GiftApi.Domain.Entities.Category { Id = 77, Name = "Electronics", IsDeleted = false, CreateDate = DateTime.UtcNow };
            var brand = new GiftApi.Domain.Entities.Brand { Id = 9, Name = "MegaBrand", CategoryId = cat.Id, Category = cat, IsDeleted = false };
            _db.Categories.Add(cat);
            _db.Brands.Add(brand);

            _db.Vouchers.AddRange(
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Special Gift", Description = "Something nice", Amount = 15, BrandId = brand.Id, Brand = brand, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Other", Description = "Electronics discount", Amount = 20, IsActive = true, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Brand Search", Description = "Desc", Amount = 30, BrandId = brand.Id, Brand = brand, IsActive = true, CreateDate = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var query = new SearchVouchersQuery { Term = "mega" };
            var byBrand = await _handler.Handle(query, CancellationToken.None);
            Assert.IsTrue(byBrand.Items.Count >= 2);

            query.Term = "electronics";
            var byCategoryOrDesc = await _handler.Handle(query, CancellationToken.None);
            Assert.IsTrue(byCategoryOrDesc.Items.Any(i => i.Title == "Other"));

            query.Term = "special";
            var byTitle = await _handler.Handle(query, CancellationToken.None);
            Assert.AreEqual(1, byTitle.TotalCount);
            Assert.AreEqual("Special Gift", byTitle.Items.Single().Title);
        }

        [Test]
        public async Task Should_Not_Return_Inactive_Or_Deleted()
        {
            _db.Vouchers.AddRange(
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Active", Description = "D", Amount = 10, IsActive = true, IsDeleted = false, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Inactive", Description = "D", Amount = 10, IsActive = false, IsDeleted = false, CreateDate = DateTime.UtcNow },
                new Domain.Entities.Voucher { Id = Guid.NewGuid(), Title = "Deleted", Description = "D", Amount = 10, IsActive = true, IsDeleted = true, CreateDate = DateTime.UtcNow }
            );
            await _db.SaveChangesAsync();

            var res = await _handler.Handle(new SearchVouchersQuery(), CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.AreEqual("Active", res.Items.Single().Title);
        }

        [Test]
        public async Task Pagination_Should_Work()
        {
            for (int i = 1; i <= 22; i++)
            {
                _db.Vouchers.Add(new Domain.Entities.Voucher
                {
                    Id = Guid.NewGuid(),
                    Title = $"Voucher {i}",
                    Description = "D",
                    Amount = i,
                    IsActive = true,
                    CreateDate = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            var query = new SearchVouchersQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 5 }
            };

            var res = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(22, res.TotalCount);
            Assert.AreEqual(5, res.Items.Count);
            Assert.AreEqual("Voucher 6", res.Items.First().Title);
        }

        [Test]
        public async Task Deleted_Brand_Should_Not_Project_Brand()
        {
            var brand = new GiftApi.Domain.Entities.Brand { Id = 50, Name = "DeadBrand", IsDeleted = true };
            _db.Brands.Add(brand);
            _db.Vouchers.Add(new Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "VoucherX",
                Description = "D",
                Amount = 10,
                IsActive = true,
                BrandId = brand.Id,
                Brand = brand,
                CreateDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var res = await _handler.Handle(new SearchVouchersQuery(), CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.IsNull(res.Items.Single().Brand);
        }

        [Test]
        public async Task Deleted_Category_Should_Not_Project_Category()
        {
            var cat = new GiftApi.Domain.Entities.Category { Id = 99, Name = "Cat99", IsDeleted = true, CreateDate = DateTime.UtcNow };
            var brand = new GiftApi.Domain.Entities.Brand { Id = 77, Name = "Brand77", CategoryId = cat.Id, Category = cat, IsDeleted = false };
            _db.Categories.Add(cat);
            _db.Brands.Add(brand);
            _db.Vouchers.Add(new Domain.Entities.Voucher
            {
                Id = Guid.NewGuid(),
                Title = "VoucherCat",
                Description = "D",
                Amount = 25,
                IsActive = true,
                BrandId = brand.Id,
                Brand = brand,
                CreateDate = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            var res = await _handler.Handle(new SearchVouchersQuery(), CancellationToken.None);

            Assert.IsTrue(res.Success);
            Assert.AreEqual(1, res.TotalCount);
            Assert.IsNotNull(res.Items.Single().Brand);
            Assert.IsNull(res.Items.Single().Category);
        }
    }
}