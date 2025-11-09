using GiftApi.Application.Features.Brand.Queries.GetWithCategory;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class GetBrandWithCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private GetBrandWithCategoryHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _brandRepository = new BrandRepository(_db);
            _handler = new GetBrandWithCategoryHandler(_brandRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Id_Is_Invalid()
        {
            var query = new GetBrandWithCategoryQuery { Id = 0 };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            StringAssert.Contains("Brand ID is required", result.Message);
        }

        [Test]
        public async Task Should_Return_NotFound_When_Brand_Does_Not_Exist()
        {
            var query = new GetBrandWithCategoryQuery { Id = 999 };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            StringAssert.Contains("Brand not found", result.Message);
        }

        [Test]
        public async Task Should_Return_Brand_With_Category_Successfully()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "CategoryX",
                Description = "Category Description",
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "BrandX",
                Description = "Brand Description",
                CategoryId = category.Id,
                CreateDate = DateTime.UtcNow
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var query = new GetBrandWithCategoryQuery { Id = brand.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(brand.Id, result.Id);
            Assert.AreEqual(brand.Name, result.Name);
            Assert.IsNotNull(result.Category);
            Assert.AreEqual(category.Id, result.Category.Id);
            Assert.AreEqual(category.Name, result.Category.Name);
        }
    }
}
