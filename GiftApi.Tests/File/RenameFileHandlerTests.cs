using GiftApi.Application.Features.File.Commands.Rename;
using GiftApi.Application.Interfaces;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class RenameFileHandlerTests
    {
        private ApplicationDbContext _db;
        private IFileRepository _fileRepository;
        private RenameFileHandler _handler;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
                .Options;

            _db = new ApplicationDbContext(options);
            _fileRepository = new FileRepository(_db);
            _handler = new RenameFileHandler(_fileRepository);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_RenameFile_When_FileExists()
        {
            var file = new GiftApi.Domain.Entities.File
            {
                Id = 1,
                FileName = "OldName.txt",
                FileUrl = "/files/OldName.txt"
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();

            var command = new RenameFileCommand
            {
                FileId = file.Id,
                NewFileName = "NewName.txt"
            };

            var result = await _handler.Handle(command, default);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(file.Id, result.FileId);
            Assert.AreEqual("NewName.txt", result.NewFileName);
            Assert.AreEqual("/files/OldName.txt", result.FileUrl);
            Assert.AreEqual("File renamed successfully", result.Message);

            var updatedFile = await _db.Files.FindAsync(file.Id);
            Assert.AreEqual("NewName.txt", updatedFile.FileName);
        }

        [Test]
        public async Task Should_ReturnFailure_When_FileDoesNotExist()
        {
            var command = new RenameFileCommand
            {
                FileId = 999,
                NewFileName = "NewName.txt"
            };

            var result = await _handler.Handle(command, default);

            Assert.IsFalse(result.Success);
            Assert.AreEqual("File not found", result.Message);
        }
    }
}
