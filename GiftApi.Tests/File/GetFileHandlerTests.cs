using GiftApi.Application.Features.File.Queries.Get;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class GetFileHandlerTests
    {
        private ApplicationDbContext _db;
        private IFileRepository _fileRepository;
        private GetFileHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "GiftApi_File_TestDb")
                .Options;

            _db = new ApplicationDbContext(options);
            _fileRepository = new FileRepository(_db);
            _handler = new GetFileHandler(_fileRepository);

            var testFile = new Domain.Entities.File
            {
                Id = new int(),
                FileName = "test.png",
                FileUrl = "https://fake.imagekit.io/test.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow,
                UserId = Guid.NewGuid(),
                MainImage = true
            };

            _db.Files.Add(testFile);
            _db.SaveChanges();
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_File_When_Exists()
        {
            var fileInDb = await _db.Files.FirstAsync();

            var query = new GetFileQuery { FileId = fileInDb.Id };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Id, Is.EqualTo(fileInDb.Id));
            Assert.That(result.FileName, Is.EqualTo(fileInDb.FileName));
            Assert.That(result.FileUrl, Is.EqualTo(fileInDb.FileUrl));
            Assert.That(result.FileType, Is.EqualTo(fileInDb.FileType));
            Assert.That(result.MainImage, Is.EqualTo(fileInDb.MainImage));
        }

        [Test]
        public async Task Should_Return_NotFound_When_File_Does_Not_Exist()
        {
            var query = new GetFileQuery { FileId = new int() };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Does.Contain("not found"));
        }
    }
}
