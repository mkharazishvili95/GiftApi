using GiftApi.Application.Features.Manage.Brand.Commands.Create;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class CreateBrandHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private ICategoryRepository _categoryRepository;
        private IUnitOfWork _unitOfWork;
        private CreateBrandHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _brandRepository = new BrandRepository(_db);
            _categoryRepository = new CategoryRepository(_db);
            _unitOfWork = new UnitOfWork(_db);

            _handler = new CreateBrandHandler(_unitOfWork, _brandRepository, _categoryRepository);
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
            var command = new CreateBrandCommand
            {
                Name = "",
                Description = "Some description",
                LogoUrl = "logo.png",
                Website = "https://example.com",
                CategoryId = 1
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand name is required"));
        }

        [Test]
        public async Task Should_Create_Brand_Successfully()
        {
            var category = new Domain.Entities.Category { Name = "Electronics" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var command = new CreateBrandCommand
            {
                Name = "BrandX",
                Description = "Description",
                LogoUrl = "logo.png",
                Website = "https://example.com",
                CategoryId = category.Id
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("BrandX", result.Name);

            var brandInDb = await _db.Brands.FirstOrDefaultAsync(b => b.Id == result.Id);
            Assert.IsNotNull(brandInDb);
            Assert.AreEqual("BrandX", brandInDb.Name);
            Assert.AreEqual("Description", brandInDb.Description);
            Assert.AreEqual("logo.png", brandInDb.LogoUrl);
            Assert.AreEqual("https://example.com", brandInDb.Website);
            Assert.IsFalse(brandInDb.IsDeleted);
        }
    }
}
