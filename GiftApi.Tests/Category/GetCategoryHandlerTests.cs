using GiftApi.Application.Features.Category.Queries.Get;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class GetCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _categoryRepository;
        private GetCategoryHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _categoryRepository = new CategoryRepository(_db);
            _handler = new GetCategoryHandler(_categoryRepository);
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
            var query = new GetCategoryQuery { Id = 0 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category ID is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
        {
            var query = new GetCategoryQuery { Id = 999 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category not found"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_Is_Deleted()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "DeletedCategory",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var query = new GetCategoryQuery { Id = category.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category not found"));
        }

        [Test]
        public async Task Should_Return_Category_Successfully()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "ActiveCategory",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var query = new GetCategoryQuery { Id = category.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(category.Id, result.Id);
            Assert.AreEqual(category.Name, result.Name);
            Assert.AreEqual(category.Description, result.Description);
            Assert.AreEqual(category.Logo, result.Logo);
        }
    }
}
