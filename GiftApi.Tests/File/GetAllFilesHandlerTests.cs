using GiftApi.Application.Common.Models;
using GiftApi.Application.Features.File.Queries.GetAll;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class GetAllFilesHandlerTests
    {
        private ApplicationDbContext _db;
        private IFileRepository _fileRepository;
        private GetAllFilesHandler _handler;
        private Guid _userA;
        private Guid _userB;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("FilesDb_" + Guid.NewGuid())
                .Options;

            _db = new ApplicationDbContext(options);
            _fileRepository = new FileRepository(_db);
            _handler = new GetAllFilesHandler(_fileRepository);
            _userA = Guid.NewGuid();
            _userB = Guid.NewGuid();

            var now = DateTime.UtcNow;

            var files = new[]
            {
                new GiftApi.Domain.Entities.File { FileName = "image_1.png", FileType = FileType.Image, UserId = _userA, MainImage = true,  UploadDate = now.AddMinutes(-30), IsDeleted = false },
                new GiftApi.Domain.Entities.File { FileName = "video_intro.mp4", FileType = FileType.Video, UserId = _userA, MainImage = false, UploadDate = now.AddMinutes(-25), IsDeleted = false },
                new GiftApi.Domain.Entities.File { FileName = "doc_specs.pdf", FileType = FileType.Document, UserId = _userB, MainImage = false, UploadDate = now.AddMinutes(-20), IsDeleted = false },
                new GiftApi.Domain.Entities.File { FileName = "image_banner.jpg", FileType = FileType.Image, UserId = _userB, MainImage = true,  UploadDate = now.AddMinutes(-15), IsDeleted = false },
                new GiftApi.Domain.Entities.File { FileName = "temp_deleted.png", FileType = FileType.Image, UserId = _userA, MainImage = false, UploadDate = now.AddMinutes(-10), IsDeleted = true }
            };

            _db.Files.AddRange(files);
            await _db.SaveChangesAsync();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_ReturnAllNonDeleted()
        {
            var query = new GetAllFilesQuery { Pagination = new PaginationModel { Page = 1, PageSize = 20 } };
            var result = await _handler.Handle(query, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(4, result.TotalCount);
            Assert.AreEqual(4, result.Items.Count);
            Assert.False(result.Items.Any(i => i.FileName == "temp_deleted.png"));
        }

        [Test]
        public async Task Should_FilterByFileNameSubstring()
        {
            var query = new GetAllFilesQuery { FileName = "image", Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(2, result.TotalCount);
            CollectionAssert.AreEquivalent(new[] { "image_1.png", "image_banner.jpg" }, result.Items.Select(i => i.FileName));
        }

        [Test]
        public async Task Should_FilterByFileType()
        {
            var query = new GetAllFilesQuery { FileType = FileType.Video, Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual("video_intro.mp4", result.Items.Single().FileName);
        }

        [Test]
        public async Task Should_FilterByUserId()
        {
            var query = new GetAllFilesQuery { UserId = _userB, Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(2, result.TotalCount);
            Assert.True(result.Items.All(i => i.UserId == _userB));
        }

        [Test]
        public async Task Should_FilterByMainImage()
        {
            var query = new GetAllFilesQuery { MainImage = true, Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(2, result.TotalCount);
            Assert.True(result.Items.All(i => i.MainImage == true));
        }

        [Test]
        public async Task Should_FilterByDateRange()
        {
            var all = await _handler.Handle(new GetAllFilesQuery { Pagination = new PaginationModel { Page = 1, PageSize = 10 } }, default);
            var earliest = all.Items.Min(i => i.UploadDate);
            var latest = all.Items.Max(i => i.UploadDate);

            var midRangeStart = earliest!.Value.AddMinutes(5);
            var midRangeEnd = latest!.Value.AddMinutes(-5);

            var query = new GetAllFilesQuery
            {
                UploadDateFrom = midRangeStart,
                UploadDateTo = midRangeEnd,
                Pagination = new PaginationModel { Page = 1, PageSize = 10 }
            };

            var result = await _handler.Handle(query, default);

            Assert.AreEqual(2, result.TotalCount);
            Assert.True(result.Items.All(i => i.UploadDate >= midRangeStart && i.UploadDate <= midRangeEnd));
        }

        [Test]
        public async Task Should_PaginateResults()
        {
            var page1 = await _handler.Handle(new GetAllFilesQuery { Pagination = new PaginationModel { Page = 1, PageSize = 2 } }, default);
            var page2 = await _handler.Handle(new GetAllFilesQuery { Pagination = new PaginationModel { Page = 2, PageSize = 2 } }, default);

            Assert.AreEqual(4, page1.TotalCount);
            Assert.AreEqual(2, page1.Items.Count);
            Assert.AreEqual(2, page2.Items.Count);

            var idsUnion = page1.Items.Select(i => i.Id).Concat(page2.Items.Select(i => i.Id)).Distinct().Count();
            Assert.AreEqual(4, idsUnion);
        }

        [Test]
        public async Task Should_NormalizeInvalidPagination()
        {
            var query = new GetAllFilesQuery { Pagination = new PaginationModel { Page = 0, PageSize = -5 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(4, result.Items.Count);
            Assert.AreEqual(4, result.TotalCount);
        }

        [Test]
        public async Task Should_ReturnEmpty_When_NoMatch()
        {
            var query = new GetAllFilesQuery { FileName = "___nope___", Pagination = new PaginationModel { Page = 1, PageSize = 10 } };
            var result = await _handler.Handle(query, default);

            Assert.AreEqual(0, result.TotalCount);
            Assert.AreEqual(0, result.Items.Count);
            Assert.IsTrue(result.Success);
        }
    }
}