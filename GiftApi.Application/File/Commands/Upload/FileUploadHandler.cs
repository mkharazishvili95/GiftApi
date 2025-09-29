using GiftApi.Application.Configuration;
using GiftApi.Application.User.Queries.Get;
using GiftApi.Common.Enums.File;
using GiftApi.Infrastructure.Data;
using Imagekit.Sdk;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace GiftApi.Application.File.Commands.Upload
{
    public class FileUploadHandler : IRequestHandler<FileUploadCommand, FileUploadResponse>
    {
        readonly ApplicationDbContext _db;
        readonly ImagekitClient _imageKitClient;
        readonly IMediator _mediator;
        readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadHandler(
            ApplicationDbContext db,
            IOptions<ImageKitSettings> imageKitOptions,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
            var settings = imageKitOptions.Value;
            _imageKitClient = new ImagekitClient(
                settings.PublicKey,
                settings.PrivateKey,
                settings.UrlEndpoint
            );
        }

        public async Task<FileUploadResponse> Handle(FileUploadCommand request, CancellationToken cancellationToken)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return new FileUploadResponse
                {
                    Success = false,
                    StatusCode = 401,
                    UserMessage = "User is not authenticated."
                };
            }

            var user = await _mediator.Send(new GetCurrentUserQuery());

            if ((bool)!user.Success)
                return new FileUploadResponse
                {
                    Success = false,
                    StatusCode = user.StatusCode,
                    UserMessage = user.UserMessage
                };

            if (string.IsNullOrWhiteSpace(request.FileContent) || string.IsNullOrWhiteSpace(request.FileName))
                throw new ArgumentException("FileName and FileContent must be provided");

            string base64Data = request.FileContent;
            string extension = request.FileType switch
            {
                FileType.Image => ".jpeg",
                FileType.Video => ".mp4",
                FileType.Document => ".pdf",
                _ => ".bin"
            };

            if (request.FileContent.StartsWith("data:"))
            {
                var parts = request.FileContent.Split(',', 2);
                if (parts.Length == 2)
                {
                    var mime = parts[0].Split(':', ';')[1];
                    extension = mime switch
                    {
                        "image/jpeg" => ".jpg",
                        "image/png" => ".png",
                        "image/gif" => ".gif",
                        "video/mp4" => ".mp4",
                        "application/pdf" => ".pdf",
                        _ => ".bin"
                    };
                    base64Data = parts[1];
                }
            }

            string uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var uploadRequest = new FileCreateRequest
            {
                fileName = uniqueFileName,
                folder = "/gifts",
                useUniqueFileName = true,
                file = base64Data,
                checks = null,
                tags = null,
                customCoordinates = null,
                responseFields = null,
                isPrivateFile = false,
                overwriteFile = false,
                customMetadata = null,
                webhookUrl = null,
                overwriteTags = false,
                overwriteCustomMetadata = false
            };

            dynamic uploadResult;
            try
            {
                uploadResult = await _imageKitClient.UploadAsync(uploadRequest);
            }
            catch (Exception ex)
            {
                return new FileUploadResponse
                {
                    Success = false,
                    StatusCode = 500,
                    UserMessage = $"File upload failed: {ex.Message}"
                };
            }

            string fileUrl = uploadResult.url;

            var fileEntity = new GiftApi.Core.Entities.File
            {
                FileName = request.FileName,
                FileUrl = fileUrl,
                FileType = request.FileType,
                UploadDate = DateTime.UtcNow.AddHours(4)
            };

            _db.Files.Add(fileEntity);
            await _db.SaveChangesAsync(cancellationToken);

            return new FileUploadResponse
            {
                Id = fileEntity.Id,
                FileName = fileEntity.FileName,
                FileUrl = fileEntity.FileUrl,
                FileType = fileEntity.FileType,
                Success = true,
                StatusCode = 200,
                UserMessage = "File uploaded successfully"
            };
        }
    }
}
