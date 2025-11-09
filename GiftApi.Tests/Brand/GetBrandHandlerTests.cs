using GiftApi.Application.Features.Brand.Queries.Get;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class GetBrandHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private GetBrandHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _brandRepository = new BrandRepository(_db);
            _handler = new GetBrandHandler(_brandRepository);
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
            var query = new GetBrandQuery { Id = 0 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand ID is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Brand_Does_Not_Exist()
        {
            var query = new GetBrandQuery { Id = 999 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand not found"));
        }

        [Test]
        public async Task Should_Return_Brand_Successfully()
        {
            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "Nike",
                Description = "Sport brand",
                LogoUrl = "logo.png",
                Website = "https://nike.com",
                CategoryId = 1,
                CreateDate = DateTime.UtcNow
            };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var query = new GetBrandQuery { Id = brand.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(brand.Id, result.Id);
            Assert.AreEqual(brand.Name, result.Name);
            Assert.AreEqual(brand.Description, result.Description);
            Assert.AreEqual(brand.LogoUrl, result.LogoUrl);
            Assert.AreEqual(brand.Website, result.Website);
            Assert.AreEqual(brand.CategoryId, result.CategoryId);
        }
    }
}
