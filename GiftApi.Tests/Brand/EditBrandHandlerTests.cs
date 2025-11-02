using GiftApi.Application.Features.Manage.Brand.Commands.Edit;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class EditBrandHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private ICategoryRepository _categoryRepository;
        private IUnitOfWork _unitOfWork;
        private EditBrandHandler _handler;

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

            _handler = new EditBrandHandler(_brandRepository, _categoryRepository, _unitOfWork);
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
            var command = new EditBrandCommand { Id = 0, Name = "BrandX" };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand Id is required"));
        }

        [Test]
        public async Task Should_Edit_Brand_Successfully()
        {
            var category = new Domain.Entities.Category { Name = "Electronics" };
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "OldName",
                CategoryId = category.Id,
                CreateDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var command = new EditBrandCommand
            {
                Id = brand.Id,
                Name = "NewName",
                Description = "Updated description",
                LogoUrl = "logo.png",
                Website = "https://example.com",
                CategoryId = category.Id
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand edited successfully"));

            var updatedBrand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == brand.Id);
            Assert.AreEqual("NewName", updatedBrand.Name);
            Assert.AreEqual("Updated description", updatedBrand.Description);
            Assert.AreEqual("logo.png", updatedBrand.LogoUrl);
            Assert.AreEqual("https://example.com", updatedBrand.Website);
        }
    }
}
