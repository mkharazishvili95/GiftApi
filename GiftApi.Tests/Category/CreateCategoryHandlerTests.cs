using GiftApi.Application.Features.Manage.Category.Commands.Create;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class CreateCategoryHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _repository;
        private IUnitOfWork _unitOfWork;
        private CreateCategoryHandler _handler;

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
            _handler = new CreateCategoryHandler(_repository, _unitOfWork);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Name_Is_Empty()
        {
            var command = new CreateCategoryCommand
            {
                Name = "",
                Description = "Some description",
                Logo = "logo.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Category name is required"));
        }

        [Test]
        public async Task Should_Create_Category_Successfully()
        {
            var command = new CreateCategoryCommand
            {
                Name = "Electronics",
                Description = "Electronic devices",
                Logo = "electronics.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(201, result.StatusCode);
            Assert.That(result.Message, Does.Contain("created successfully"));
            Assert.AreNotEqual(new int(), result.Id);

            var categoryInDb = await _db.Categories.FirstOrDefaultAsync(c => c.Id == result.Id);
            Assert.IsNotNull(categoryInDb);
            Assert.AreEqual("Electronics", categoryInDb.Name);
            Assert.AreEqual("Electronic devices", categoryInDb.Description);
            Assert.AreEqual("electronics.png", categoryInDb.Logo);
            Assert.IsFalse(categoryInDb.IsDeleted);
        }
    }
}
