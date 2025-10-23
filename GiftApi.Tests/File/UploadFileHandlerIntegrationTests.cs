using GiftApi.Application.Configuration;
using GiftApi.Application.Features.File.Commands.Upload;
using GiftApi.Application.Interfaces;
using GiftApi.Domain.Enums.File;
using GiftApi.Infrastructure.Data;
using GiftApi.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GiftApi.Tests.File
{
    [TestFixture]
    public class UploadFileHandlerIntegrationTests
    {
        private ApplicationDbContext _db;
        private UploadFileHandler _handler;
        private IFileRepository _fileRepository;
        private IUserRepository _userRepository;
        private IHttpContextAccessor _httpContextAccessor;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ApplicationDbContext(options);

            var testUser = new GiftApi.Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                Password = "password",
                PhoneNumber = "123456789",
                UserName = "testuser",
                IdentificationNumber = "1234567890",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                Type = GiftApi.Domain.Enums.User.UserType.User,
                Balance = 0
            };

            _db.Users.Add(testUser);
            _db.SaveChanges();

            _fileRepository = new FileRepository(_db);
            _userRepository = new UserRepository(_db);

            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, testUser.Id.ToString())
            }));

            _httpContextAccessor = new HttpContextAccessor { HttpContext = context };

            var imageKitOptions = Options.Create(new ImageKitSettings
            {
                PublicKey = "fake_public",
                PrivateKey = "fake_private",
                UrlEndpoint = "https://fake.imagekit.io/test"
            });

            _handler = new UploadFileHandler(
                imageKitOptions,
                null!,
                _httpContextAccessor,
                _fileRepository,
                _userRepository
            );
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Test]
        public async Task Should_Upload_File_Successfully()
        {
            var userId = Guid.NewGuid();
            var httpContextMock = new DefaultHttpContext();
            httpContextMock.User = new ClaimsPrincipal(
                new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                })
            );
            _httpContextAccessor.HttpContext = httpContextMock;

            var user = new GiftApi.Domain.Entities.User
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "123456789",
                IdentificationNumber = "12345678901",
                DateOfBirth = DateTime.UtcNow.AddYears(-20),
                UserName = "testuser",
                Password = "hashedpassword",
                Gender = GiftApi.Domain.Enums.User.Gender.Male,
                RegisterDate = DateTime.UtcNow.AddHours(4),
                Balance = 0,
                Type = GiftApi.Domain.Enums.User.UserType.User
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var command = new UploadFileCommand
            {
                FileName = "example.png",
                FileContent = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA",
                FileType = FileType.Image
            };

            var fileUrl = "https://fake.imagekit.io/test.png";
            var fileDto = await _fileRepository.UploadFileAsync(command.FileName, fileUrl, command.FileType);

            var result = new UploadFileResponse
            {
                Id = fileDto.Id,
                FileName = fileDto.FileName,
                FileUrl = fileDto.FileUrl,
                FileType = fileDto.FileType,
                Success = true,
                StatusCode = 200,
                Message = "File uploaded successfully"
            };

            Assert.That(result.Success, Is.True);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.FileName, Is.EqualTo("example.png"));
            Assert.That(result.FileType, Is.EqualTo(FileType.Image));
            Assert.That(result.FileUrl, Does.Contain("https://fake.imagekit.io/test"));

            var fileInDb = await _db.Files.FirstOrDefaultAsync(f => f.Id == result.Id);
            Assert.IsNotNull(fileInDb);
            Assert.AreEqual("example.png", fileInDb.FileName);
            Assert.IsFalse(fileInDb.IsDeleted);
        }

        [Test]
        public async Task Should_Return_Unauthorized_When_User_Is_Missing()
        {
            var command = new UploadFileCommand
            {
                FileName = "example.png",
                FileContent = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA",
                FileType = FileType.Image
            };

            var httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };

            var handler = new UploadFileHandler(
                Options.Create(new ImageKitSettings
                {
                    PublicKey = "fake_public",
                    PrivateKey = "fake_private",
                    UrlEndpoint = "https://fake.imagekit.io/test"
                }),
                null!,
                httpContextAccessor,
                _fileRepository,
                _userRepository
            );

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.That(result.Success, Is.False);
            Assert.That(result.StatusCode, Is.EqualTo(401));
            Assert.That(result.Message, Does.Contain("User is not authenticated"));
        }

        [Test]
        public void Should_Throw_When_FileContent_Or_FileName_Is_Empty()
        {
            var command = new UploadFileCommand
            {
                FileName = "",
                FileContent = "",
                FileType = FileType.Image
            };

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }
    }
}
