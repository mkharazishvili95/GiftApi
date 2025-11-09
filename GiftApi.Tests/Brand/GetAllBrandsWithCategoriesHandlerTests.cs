using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Brand.Queries.GetAllWithCategories;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class GetAllBrandsWithCategoriesHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private GetAllBrandsWithCategoriesHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _brandRepository = new BrandRepository(_db);
            _handler = new GetAllBrandsWithCategoriesHandler(_brandRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_Empty_List_When_No_Brands()
        {
            var query = new GetAllBrandsWithCategoriesQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.IsNotNull(result.Items);
            Assert.AreEqual(0, result.TotalCount);
            Assert.IsEmpty(result.Items);
        }

        [Test]
        public async Task Should_Return_All_Brands_With_Categories_Successfully()
        {
            var category1 = new GiftApi.Domain.Entities.Category
            {
                Name = "Category1",
                Description = "Desc1",
                CreateDate = DateTime.UtcNow
            };
            var category2 = new GiftApi.Domain.Entities.Category
            {
                Name = "Category2",
                Description = "Desc2",
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.AddRange(category1, category2);
            await _db.SaveChangesAsync();

            var brand1 = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand1",
                CategoryId = category1.Id,
                CreateDate = DateTime.UtcNow
            };
            var brand2 = new GiftApi.Domain.Entities.Brand
            {
                Name = "Brand2",
                CategoryId = category2.Id,
                CreateDate = DateTime.UtcNow
            };
            _db.Brands.AddRange(brand1, brand2);
            await _db.SaveChangesAsync();

            var query = new GetAllBrandsWithCategoriesQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);

            var firstBrand = result.Items.First(b => b.Id == brand1.Id);
            Assert.AreEqual(brand1.Name, firstBrand.Name);
            Assert.IsNotNull(firstBrand.Category);
            Assert.AreEqual(category1.Name, firstBrand.Category.Name);

            var secondBrand = result.Items.First(b => b.Id == brand2.Id);
            Assert.AreEqual(brand2.Name, secondBrand.Name);
            Assert.IsNotNull(secondBrand.Category);
            Assert.AreEqual(category2.Name, secondBrand.Category.Name);
        }

        [Test]
        public async Task Should_Handle_Pagination_Correctly()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Category",
                Description = "Desc",
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            for (int i = 1; i <= 5; i++)
            {
                _db.Brands.Add(new GiftApi.Domain.Entities.Brand
                {
                    Name = $"Brand{i}",
                    CategoryId = category.Id,
                    CreateDate = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            var query = new GetAllBrandsWithCategoriesQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 2 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(5, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);
            Assert.AreEqual("Brand3", result.Items[0].Name);
            Assert.AreEqual("Brand4", result.Items[1].Name);
        }
    }
}
