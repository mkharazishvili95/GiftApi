using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Category.Queries.GetAllWithBrands;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class GetAllCategoriesWithBrandsHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _categoryRepository;
        private GetAllCategoriesWithBrandsHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _categoryRepository = new CategoryRepository(_db);
            _handler = new GetAllCategoriesWithBrandsHandler(_categoryRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_EmptyList_When_NoCategories()
        {
            var query = new GetAllCategoriesWithBrandsQuery
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
        public async Task Should_Return_Categories_With_Brands()
        {
            var category1 = new GiftApi.Domain.Entities.Category
            {
                Name = "Electronics",
                Description = "Electronic devices",
                CreateDate = DateTime.UtcNow
            };
            var category2 = new GiftApi.Domain.Entities.Category
            {
                Name = "Furniture",
                Description = "Home furniture",
                CreateDate = DateTime.UtcNow
            };

            var brands1 = new List<GiftApi.Domain.Entities.Brand>
            {
                new GiftApi.Domain.Entities.Brand { Name = "Samsung", CreateDate = DateTime.UtcNow },
                new GiftApi.Domain.Entities.Brand { Name = "Apple", CreateDate = DateTime.UtcNow }
            };

            var brands2 = new List<GiftApi.Domain.Entities.Brand>
            {
                new GiftApi.Domain.Entities.Brand { Name = "Ikea", CreateDate = DateTime.UtcNow }
            };

            category1.Brands = brands1;
            category2.Brands = brands2;

            _db.Categories.AddRange(category1, category2);
            await _db.SaveChangesAsync();

            var query = new GetAllCategoriesWithBrandsQuery
            {
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);

            var electronics = result.Items.FirstOrDefault(c => c.Name == "Electronics");
            Assert.NotNull(electronics);
            Assert.AreEqual(2, electronics.Brands.Count);
            Assert.IsTrue(electronics.Brands.Any(b => b.Name == "Samsung"));
            Assert.IsTrue(electronics.Brands.Any(b => b.Name == "Apple"));

            var furniture = result.Items.FirstOrDefault(c => c.Name == "Furniture");
            Assert.NotNull(furniture);
            Assert.AreEqual(1, furniture.Brands.Count);
            Assert.AreEqual("Ikea", furniture.Brands.First().Name);
        }

        [Test]
        public async Task Should_Handle_Pagination_Correctly()
        {
            for (int i = 1; i <= 5; i++)
            {
                _db.Categories.Add(new GiftApi.Domain.Entities.Category
                {
                    Name = $"Category{i}",
                    Description = $"Description{i}",
                    CreateDate = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            var query = new GetAllCategoriesWithBrandsQuery
            {
                Pagination = new PaginationModel { Page = 2, PageSize = 2 }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(5, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);
            Assert.AreEqual("Category3", result.Items[0].Name);
            Assert.AreEqual("Category4", result.Items[1].Name);
        }
    }
}
