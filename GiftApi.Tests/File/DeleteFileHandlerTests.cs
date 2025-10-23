using GiftApi.Application.Features.File.Commands.Delete;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class DeleteFileHandlerIntegrationTests
    {
        private ApplicationDbContext _db;
        private IFileRepository _fileRepository;
        private IUnitOfWork _unitOfWork;
        private DeleteFileHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);
            _fileRepository = new FileRepository(_db);
            _unitOfWork = new UnitOfWork(_db);

            _handler = new DeleteFileHandler(_fileRepository, _unitOfWork);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Return_BadRequest_When_FileId_Is_Invalid()
        {
            var command = new DeleteFileCommand { FileId = 0 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.Message, Does.Contain("request required"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_File_Does_Not_Exist()
        {
            var command = new DeleteFileCommand { FileId = 999 };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_When_File_Already_Deleted()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "deleted.jpg",
                FileUrl = "https://fake.imagekit.io/deleted.jpg",
                FileType = FileType.Image,
                IsDeleted = true
            };

            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new DeleteFileCommand { FileId = file.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Does.Contain("not found"));
        }

        [Test]
        public async Task Should_Delete_File_Successfully()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "test.jpg",
                FileUrl = "https://fake.imagekit.io/test.jpg",
                FileType = FileType.Image,
                IsDeleted = false
            };

            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new DeleteFileCommand { FileId = file.Id };
            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Message, Does.Contain("successfully"));

            var deletedFile = await _db.Files.FindAsync(file.Id);
            Assert.That(deletedFile!.IsDeleted, Is.True);
        }
    }
}
