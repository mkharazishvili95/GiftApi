using GiftApi.Application.Features.Manage.Category.Commands.Edit;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class EditCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _repository;
        private IUnitOfWork _unitOfWork;
        private EditCategoryHandler _handler;

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
            _handler = new EditCategoryHandler(_repository, _unitOfWork);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_NotFound_When_Category_Does_Not_Exist()
        {
            var command = new EditCategoryCommand
            {
                Id = 999,
                Name = "Updated Name",
                Description = "Updated Description",
                Logo = "updated.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Category_Is_Deleted()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Deleted Category",
                Description = "Desc",
                Logo = "logo.png",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new EditCategoryCommand
            {
                Id = category.Id,
                Name = "Updated Name",
                Description = "Updated Description",
                Logo = "updated.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("deleted"));
        }

        [Test]
        public async Task Should_Edit_Category_Successfully()
        {
            var category = new GiftApi.Domain.Entities.Category
            {
                Name = "Old Name",
                Description = "Old Description",
                Logo = "old.png",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new EditCategoryCommand
            {
                Id = category.Id,
                Name = "New Name",
                Description = "New Description",
                Logo = "new.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("updated successfully"));
            Assert.AreEqual(category.Id, result.Id);
            Assert.AreEqual("New Name", result.Name);
            Assert.AreEqual("New Description", result.Description);
            Assert.AreEqual("new.png", result.Logo);

            var categoryInDb = await _db.Categories.FindAsync(category.Id);
            Assert.AreEqual("New Name", categoryInDb.Name);
            Assert.AreEqual("New Description", categoryInDb.Description);
            Assert.AreEqual("new.png", categoryInDb.Logo);
        }
    }
}
