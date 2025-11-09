using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.Category.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Category
{
    [TestFixture]
    public class GetAllCategoriesHandlerTests
    {
        private ApplicationDbContext _db;
        private ICategoryRepository _categoryRepository;
        private GetAllCategoriesHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _categoryRepository = new CategoryRepository(_db);
            _handler = new GetAllCategoriesHandler(_categoryRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_Empty_List_When_No_Categories()
        {
            var query = new GetAllCategoriesQuery { Pagination = new PaginationModel { Page = 1, PageSize = 10 } };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(0, result.TotalCount);
            Assert.IsEmpty(result.Items);
        }

        [Test]
        public async Task Should_Return_All_Categories_With_Pagination()
        {
            var categories = new List<GiftApi.Domain.Entities.Category>();
            for (int i = 1; i <= 15; i++)
            {
                categories.Add(new GiftApi.Domain.Entities.Category
                {
                    Name = $"Category {i}",
                    Description = $"Description {i}",
                    CreateDate = DateTime.UtcNow
                });
            }
            _db.Categories.AddRange(categories);
            await _db.SaveChangesAsync();

            var query = new GetAllCategoriesQuery { Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual(15, result.TotalCount);
            Assert.AreEqual(10, result.Items.Count);
            Assert.AreEqual("Category 1", result.Items.First().Name);

            query.Pagination.Page = 2;
            result = await _handler.Handle(query, CancellationToken.None);

            Assert.AreEqual(5, result.Items.Count);
            Assert.AreEqual("Category 11", result.Items.First().Name);
        }
    }
}
