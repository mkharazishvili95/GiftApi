using GiftApi.Application.Features.Manage.Brand.Commands.Restore;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class RestoreBrandHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private IUnitOfWork _unitOfWork;
        private RestoreBrandHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            _brandRepository = new BrandRepository(_db);
            _unitOfWork = new UnitOfWork(_db);
            _handler = new RestoreBrandHandler(_unitOfWork, _brandRepository);
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
            var command = new RestoreBrandCommand { Id = 0 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand Id is required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_Brand_Does_Not_Exist()
        {
            var command = new RestoreBrandCommand { Id = 999 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(404, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_Brand_Is_Not_Deleted()
        {
            var brand = new GiftApi.Domain.Entities.Brand { Name = "BrandX", IsDeleted = false, CreateDate = DateTime.UtcNow };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            var command = new RestoreBrandCommand { Id = brand.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(400, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand is not deleted"));
        }

        [Test]
        public async Task Should_Restore_Brand_Successfully()
        {
            var brand = new GiftApi.Domain.Entities.Brand
            {
                Name = "BrandX",
                IsDeleted = true,
                CreateDate = DateTime.UtcNow,
                DeleteDate = DateTime.UtcNow.AddHours(-1) 
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();

            _db.Entry(brand).State = EntityState.Detached;

            var command = new RestoreBrandCommand { Id = brand.Id };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.That(result.Message, Does.Contain("Brand restored successfully"));

            var restoredBrand = await _db.Brands.FirstOrDefaultAsync(b => b.Id == brand.Id);
            Assert.IsFalse(restoredBrand.IsDeleted);
        }

    }
}
