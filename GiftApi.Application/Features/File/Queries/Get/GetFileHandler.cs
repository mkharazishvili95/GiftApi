using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.File.Queries.Get
{
    public class GetFileHandler : IRequestHandler<GetFileQuery, GetFileResponse>
    {
        readonly IFileRepository _fileRepository;
        public GetFileHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<GetFileResponse> Handle(GetFileQuery request, CancellationToken cancellationToken)
        {
            var file = await _fileRepository.GetFileAsync(request.FileId);

            if (file == null)
                return new GetFileResponse { Message = "File not found.", Success = false, StatusCode = 404 };
            
            var response = new GetFileResponse
            {
                StatusCode = 200,
                Success = true,
                Id = file.Id,
                FileName = file.FileName,
                FileUrl = file.FileUrl,
                FileType = file.FileType,
                UploadDate = file.UploadDate,
                UserId = file.UserId,
                MainImage = file.MainImage
            };

            return response;
        }
    }
}
