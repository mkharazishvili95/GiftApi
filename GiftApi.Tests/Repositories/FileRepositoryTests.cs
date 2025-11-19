using FluentAssertions;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using GiftApi.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GiftApi.Tests.Repositories
{
    [TestFixture]
    public class FileRepositoryTests
    {
        ApplicationDbContext _db = null!;
        FileRepository _repo = null!;
        int _fileId;

        [SetUp]
        public async Task SetUp()
        {
            _db = DbContextHelper.GetInMemoryDbContext();
            _repo = new FileRepository(_db);

            var file = new GiftApi.Domain.Entities.File
            {
                FileName = "init.png",
                FileUrl = "http://example.com/init.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow,
                IsDeleted = false
            };
            _db.Files.Add(file);
            await _db.SaveChangesAsync();
            _fileId = file.Id;
        }

        [TearDown]
        public void TearDown()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task GetFileAsync_Should_Return_File()
        {
            var f = await _repo.GetFileAsync(_fileId);
            f.Should().NotBeNull();
            f!.FileName.Should().Be("init.png");
        }

        [Test]
        public async Task GetFileAsync_Should_Return_Null_For_Deleted()
        {
            var f = await _db.Files.FindAsync(_fileId);
            f!.IsDeleted = true;
            _db.Update(f);
            await _db.SaveChangesAsync();
            (await _repo.GetFileAsync(_fileId)).Should().BeNull();
        }

        [Test]
        public async Task GetFileAsync_Should_Return_Null_For_Missing()
        {
            (await _repo.GetFileAsync(9999)).Should().BeNull();
        }

        [Test]
        public async Task UploadFileAsync_Should_Create_File()
        {
            var dto = await _repo.UploadFileAsync("logo.png", "http://cdn/logo.png", FileType.Image);
            dto.Should().NotBeNull();
            dto.FileName.Should().Be("logo.png");
            dto.FileUrl.Should().Be("http://cdn/logo.png");
            dto.FileType.Should().Be(FileType.Image);
            (await _db.Files.CountAsync()).Should().Be(2);
        }

        [Test]
        public async Task UploadFileAsync_Should_Throw_For_Invalid()
        {
            await FluentActions.Invoking(() => _repo.UploadFileAsync("", "url", FileType.Image))
                .Should().ThrowAsync<ArgumentException>();
            await FluentActions.Invoking(() => _repo.UploadFileAsync("name", "", FileType.Image))
                .Should().ThrowAsync<ArgumentException>();
            await FluentActions.Invoking(() => _repo.UploadFileAsync("name", "url", null))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task DeleteFileAsync_Should_SoftDelete()
        {
            var ok = await _repo.DeleteFileAsync(_fileId);
            ok.Should().BeTrue();
            await _db.SaveChangesAsync();
            var f = await _db.Files.FindAsync(_fileId);
            f!.IsDeleted.Should().BeTrue();
            f.DeleteDate.Should().NotBeNull();
            (await _repo.GetFileAsync(_fileId)).Should().BeNull();
        }

        [Test]
        public async Task DeleteFileAsync_Should_Return_False_For_Missing()
        {
            (await _repo.DeleteFileAsync(12345)).Should().BeFalse();
        }

        [Test]
        public async Task DeleteFileAsync_Should_Return_False_For_Already_Deleted()
        {
            var f = await _db.Files.FindAsync(_fileId);
            f!.IsDeleted = true;
            _db.Update(f);
            await _db.SaveChangesAsync();
            (await _repo.DeleteFileAsync(_fileId)).Should().BeFalse();
        }

        [Test]
        public async Task EditFile_Should_Update_Metadata()
        {
            var f = await _db.Files.FindAsync(_fileId);
            f!.FileName = "changed.png";
            f.FileUrl = "http://cdn/changed.png";
            await _repo.EditFile(f);
            var tracked = await _db.Files.FindAsync(_fileId);
            tracked!.FileName.Should().Be("changed.png");
            tracked.FileUrl.Should().Be("http://cdn/changed.png");
        }

        [Test]
        public async Task EditFile_Should_NoOp_When_Deleted()
        {
            var f = await _db.Files.FindAsync(_fileId);
            f!.IsDeleted = true;
            _db.Update(f);
            await _db.SaveChangesAsync();
            f.FileName = "new.png";
            await _repo.EditFile(f);
            var tracked = await _db.Files.FindAsync(_fileId);
            tracked!.FileName.Should().Be("new.png");
        }

        [Test]
        public async Task GetAll_Should_Return_Only_NotDeleted()
        {
            var f = new GiftApi.Domain.Entities.File
            {
                FileName = "del.png",
                FileUrl = "http://cdn/del.png",
                FileType = FileType.Image,
                UploadDate = DateTime.UtcNow,
                IsDeleted = true,
                DeleteDate = DateTime.UtcNow
            };
            _db.Files.Add(f);
            await _db.SaveChangesAsync();

            var list = await _repo.GetAll();
            list.Should().NotBeNull();
            list!.Count.Should().Be(1);
            list.First().FileName.Should().Be("init.png");
        }
    }
}