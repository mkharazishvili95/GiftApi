using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.File.Commands.Delete
{
    public class DeleteFileHandler : IRequestHandler<DeleteFileCommand, DeleteFileResponse>
    {
        readonly IFileRepository _fileRepository;
        readonly IUnitOfWork _unitOfWork;
        public DeleteFileHandler(IFileRepository fileRepository, IUnitOfWork unitOfWork)
        {
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteFileResponse> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            if(request.FileId <= 0)
                return new DeleteFileResponse { Message = "request required.", Success = false, StatusCode = 400 };

            var file = await _fileRepository.GetFileAsync(request.FileId);

            if(file == null)
                return new DeleteFileResponse { Message = "File not found.", Success = false, StatusCode = 404 };

            if(file.IsDeleted)
                return new DeleteFileResponse { Message = "File already deleted.", Success = false, StatusCode = 400 };

            var result =  await _fileRepository.DeleteFileAsync(request.FileId);

            if(!result)
                return new DeleteFileResponse { Message = "Failed to delete file.", Success = false, StatusCode = 500 };

            await _unitOfWork.SaveChangesAsync();
            return new DeleteFileResponse { Message = "File deleted successfully.", Success = true, StatusCode = 200 };
        }
    }
}
