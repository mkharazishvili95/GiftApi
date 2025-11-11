using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.File.Commands.Rename
{
    public class RenameFileHandler : IRequestHandler<RenameFileCommand, RenameFileResponse>
    {
        readonly IFileRepository _fileRepository;
        public RenameFileHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<RenameFileResponse> Handle(RenameFileCommand request, CancellationToken cancellationToken)
        {
            var file = await _fileRepository.GetFileAsync(request.FileId);

            if (file == null)
                return new RenameFileResponse { Success = false, Message = "File not found" };

            file.FileName = request.NewFileName;

            await _fileRepository.EditFile(file);

            return new RenameFileResponse
            {
                StatusCode = 200,
                Success = true,
                FileId = file.Id,
                NewFileName = file.FileName,
                FileUrl = file.FileUrl,
                Message = "File renamed successfully"
            };
        }
    }
}
