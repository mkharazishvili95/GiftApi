using GiftApi.Application.Features.Manage.Category.Commands.Restore;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class RestoreCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _repository;
        private IUnitOfWork _unitOfWork;
        private RestoreCategoryHandler _handler;

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
            _handler = new RestoreCategoryHandler(_unitOfWork, _repository);
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
            var command = new RestoreCategoryCommand { Id = 0 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
        {
            var command = new RestoreCategoryCommand { Id = 999 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Category_Is_Not_Deleted()
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

            var command = new RestoreCategoryCommand { Id = category.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not deleted"));
        }

        [Test]
        public async Task Should_Restore_Category_Successfully()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Deleted",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow.AddHours(-1),
                CreateDate = DateTime.UtcNow.AddHours(-2)
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new RestoreCategoryCommand { Id = category.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("restored successfully"));

            var categoryInDb = await _db.Categories.FindAsync(category.Id);
            Assert.IsFalse(categoryInDb.IsDeleted);
            Assert.IsNull(categoryInDb.DeleteDate);
            Assert.IsNotNull(categoryInDb.UpdateDate);
        }
    }
}
