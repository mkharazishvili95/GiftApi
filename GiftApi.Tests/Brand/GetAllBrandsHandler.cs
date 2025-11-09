using GiftApi.Application.Features.Brand.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Brand
{
    [TestFixture]
    public class GetAllBrandsHandlerTests
    {
        private ApplicationDbContext _db;
        private IBrandRepository _brandRepository;
        private GetAllBrandsHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _brandRepository = new BrandRepository(_db);
            _handler = new GetAllBrandsHandler(_brandRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_Empty_List_When_No_Brands_Exist()
        {
            var query = new GetAllBrandsQuery
            {
                Pagination = new()
                {
                    Page = 1,
                    PageSize = 10
                }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, result.TotalCount);
            Assert.IsEmpty(result.Items);
        }

        [Test]
        public async Task Should_Return_Paginated_Brands_List()
        {
            for (int i = 1; i <= 10; i++)
            {
                _db.Brands.Add(new GiftApi.Domain.Entities.Brand
                {
                    Name = $"Brand {i}",
                    CreateDate = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();

            var query = new GetAllBrandsQuery
            {
                Pagination = new()
                {
                    Page = 2,
                    PageSize = 3
                }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(10, result.TotalCount);
            Assert.AreEqual(3, result.Items.Count);
            Assert.AreEqual("Brand 4", result.Items.First().Name);
        }

        [Test]
        public async Task Should_Return_All_Brands_When_Pagination_Is_Large()
        {
            _db.Brands.AddRange(
                new GiftApi.Domain.Entities.Brand { Name = "A", CreateDate = DateTime.UtcNow },
                new GiftApi.Domain.Entities.Brand { Name = "B", CreateDate = DateTime.UtcNow }
            );

            await _db.SaveChangesAsync();

            var query = new GetAllBrandsQuery
            {
                Pagination = new()
                {
                    Page = 1,
                    PageSize = 100
                }
            };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(2, result.TotalCount);
            Assert.AreEqual(2, result.Items.Count);
            CollectionAssert.AreEquivalent(new[] { "A", "B" }, result.Items.Select(x => x.Name));
        }
    }
}
