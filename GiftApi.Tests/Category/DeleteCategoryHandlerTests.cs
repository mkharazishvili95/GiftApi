using GiftApi.Application.Features.Manage.Category.Commands.Delete;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class DeleteCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _repository;
        private IUnitOfWork _unitOfWork;
        private DeleteCategoryHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;

            _db = new ApplicationDbContext(options);

            _db.Categories = _db.Categories;

            _repository = new CategoryRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new DeleteCategoryHandler(_unitOfWork, _repository);
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
            var command = new DeleteCategoryCommand { Id = 0 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
        {
            var command = new DeleteCategoryCommand { Id = 999 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Category_Already_Deleted()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Deleted",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new DeleteCategoryCommand { Id = category.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("already deleted"));
        }

        [Test]
        public async Task Should_Delete_Category_Successfully()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Active",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new DeleteCategoryCommand { Id = category.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("deleted successfully"));

            var categoryInDb = await _db.Categories.FindAsync(category.Id);
            Assert.IsTrue(categoryInDb.IsDeleted);
            Assert.IsNotNull(categoryInDb.DeleteDate);
        }
    }
}
