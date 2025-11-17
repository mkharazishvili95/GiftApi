using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.File.Commands.UpdateMeta
{
    public class UpdateFileMetaHandler : IRequestHandler<UpdateFileMetaCommand, UpdateFileMetaResponse>
    {
        readonly IFileRepository _fileRepository;

        public UpdateFileMetaHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<UpdateFileMetaResponse> Handle(UpdateFileMetaCommand request, CancellationToken cancellationToken)
        {
            if (request.FileId <= 0)
                return new UpdateFileMetaResponse { Success = false, StatusCode = 400, Message = "FileId is required." };

            var file = await _fileRepository.GetFileAsync(request.FileId);
            if (file == null)
                return new UpdateFileMetaResponse { Success = false, StatusCode = 404, Message = "File not found." };

            if (request.FileName != null)
                file.FileName = request.FileName.Trim();

            if (request.FileType.HasValue)
                file.FileType = request.FileType.Value;

            if (request.MainImage.HasValue)
                file.MainImage = request.MainImage.Value;

            await _fileRepository.EditFile(file);

            return new UpdateFileMetaResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "File metadata updated successfully.",
                Id = file.Id,
                FileName = file.FileName,
                FileUrl = file.FileUrl,
                FileType = file.FileType,
                UploadDate = file.UploadDate,
                UserId = file.UserId,
                MainImage = file.MainImage
            };
        }
    }
}