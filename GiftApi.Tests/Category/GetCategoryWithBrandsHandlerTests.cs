using GiftApi.Application.Features.Category.Queries.GetWithBrands;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class GetCategoryWithBrandsHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _categoryRepository;
        private GetCategoryWithBrandsHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _categoryRepository = new CategoryRepository(_db);
            _handler = new GetCategoryWithBrandsHandler(_categoryRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_CategoryId_Invalid()
        {
            var query = new GetCategoryWithBrandsQuery { CategoryId = 0 };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category ID is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_DoesNotExist()
        {
            var query = new GetCategoryWithBrandsQuery { CategoryId = 999 };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category not found"));
        }

        [Test]
        public async Task Should_Return_Category_With_Brands()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Electronics",
                Description = "Electronic devices",
                CreateDate = DateTime.UtcNow
            };

            var brands = new List<GiftApi.Domain.Entities.Brand>
            {
                new GiftApi.Domain.Entities.Brand { Name = "Samsung", CreateDate = DateTime.UtcNow },
                new GiftApi.Domain.Entities.Brand { Name = "Apple", CreateDate = DateTime.UtcNow }
            };

            category.Brands = brands;
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var query = new GetCategoryWithBrandsQuery { CategoryId = category.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(category.Id, result.Id);
            Assert.AreEqual(2, result.Brands.Count);
            Assert.IsTrue(result.Brands.Any(b => b.Name == "Samsung"));
            Assert.IsTrue(result.Brands.Any(b => b.Name == "Apple"));
        }

        [Test]
        public async Task Should_Return_Empty_Brands_List_When_No_Brands()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Furniture",
                Description = "Home furniture",
                CreateDate = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var query = new GetCategoryWithBrandsQuery { CategoryId = category.Id };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(category.Id, result.Id);
            Assert.IsEmpty(result.Brands);
        }
    }
}
