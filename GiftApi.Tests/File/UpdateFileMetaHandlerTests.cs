using GiftApi.Application.Features.File.Commands.UpdateMeta;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class UpdateFileMetaHandlerTests
    {
        private ApplicationDbContext _db;
        private IFileRepository _fileRepository;
        private UpdateFileMetaHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"FileMetaDb_{Guid.NewGuid()}")
                .Options;

            _db = new ApplicationDbContext(options);
            _fileRepository = new FileRepository(_db);
            _handler = new UpdateFileMetaHandler(_fileRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Update_FileName_Only()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "old.png",
                FileUrl = "/files/old.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow.AddHours(4),
                MainImage = false
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new UpdateFileMetaCommand
            {
                FileId = file.Id,
                FileName = "new.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.FileName, Is.EqualTo("new.png"));
            Assert.That(result.FileType, Is.EqualTo(FileType.Image));
            Assert.That(result.MainImage, Is.False);

            var updated = await _db.Files.FindAsync(file.Id);
            Assert.That(updated!.FileName, Is.EqualTo("new.png"));
            Assert.That(updated.FileType, Is.EqualTo(FileType.Image));
            Assert.That(updated.MainImage, Is.False);
        }

        [Test]
        public async Task Should_Update_FileType_And_MainImage()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "doc1.pdf",
                FileUrl = "/files/doc1.pdf",
                FileType = FileType.Document,
                UploadDate = DateTime.UtcNow.AddHours(4),
                MainImage = false
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new UpdateFileMetaCommand
            {
                FileId = file.Id,
                FileType = FileType.Image,
                MainImage = true
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.FileType, Is.EqualTo(FileType.Image));
            Assert.That(result.MainImage, Is.True);
            Assert.That(result.FileName, Is.EqualTo("doc1.pdf"));

            var updated = await _db.Files.FindAsync(file.Id);
            Assert.That(updated!.FileType, Is.EqualTo(FileType.Image));
            Assert.That(updated.MainImage, Is.True);
            Assert.That(updated.FileName, Is.EqualTo("doc1.pdf"));
        }

        [Test]
        public async Task Should_Ignore_Null_Fields_And_Leave_File_Untouched()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "imageA.png",
                FileUrl = "/files/imageA.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow.AddHours(4),
                MainImage = false
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new UpdateFileMetaCommand
            {
                FileId = file.Id
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.FileName, Is.EqualTo("imageA.png"));
            Assert.That(result.FileType, Is.EqualTo(FileType.Image));
            Assert.That(result.MainImage, Is.False);

            var updated = await _db.Files.FindAsync(file.Id);
            Assert.That(updated!.FileName, Is.EqualTo("imageA.png"));
            Assert.That(updated.FileType, Is.EqualTo(FileType.Image));
            Assert.That(updated.MainImage, Is.False);
        }

        [Test]
        public async Task Should_Return_NotFound_When_File_Does_Not_Exist()
        {
            var command = new UpdateFileMetaCommand
            {
                FileId = 999,
                FileName = "something.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Does.Contain("File not found"));
        }

        [Test]
        public async Task Should_Return_NotFound_When_File_Is_Deleted()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "deleted.png",
                FileUrl = "/files/deleted.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow.AddHours(4),
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow.AddHours(4)
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new UpdateFileMetaCommand
            {
                FileId = file.Id,
                FileName = "newname.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Message, Does.Contain("File not found"));
        }

        [Test]
        public async Task Should_Return_BadRequest_For_Invalid_Id()
        {
            var command = new UpdateFileMetaCommand
            {
                FileId = 0,
                FileName = "some.png"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.Message, Does.Contain("FileId is required"));
        }
    }
}