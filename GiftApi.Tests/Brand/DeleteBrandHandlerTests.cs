using GiftApi.Application.Features.Manage.Brand.Commands.Delete;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class DeleteBrandHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private IUnitOfWork _unitOfWork;
        private DeleteBrandHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _brandRepository = new BrandRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new DeleteBrandHandler(_unitOfWork, _brandRepository);
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
            var command = new DeleteBrandCommand { Id = 0 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("BrandId is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Brand_Does_Not_Exist()
        {
            var command = new DeleteBrandCommand { Id = 999 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand not found"));
        }

        [Test]
        public async Task Should_Delete_Brand_Successfully()
        {
            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "BrandX",
                IsDeleted = false,
                CreateDate = DateTime.UtcNow
            };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            _db.Entry(brand).State = EntityState.Detached;

            var command = new DeleteBrandCommand { Id = brand.Id };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand deleted successfully"));

            var deletedBrand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == brand.Id);
            Assert.IsTrue(deletedBrand.IsDeleted);
        }

    }
}
