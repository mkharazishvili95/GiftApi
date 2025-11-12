using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.File.Queries.GetAll
{
    public class GetAllFilesHandler : IRequestHandler<GetAllFilesQuery, GetAllFilesResponse>
    {
        readonly IFileRepository _fileRepository;

        public GetAllFilesHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<GetAllFilesResponse> Handle(GetAllFilesQuery request, CancellationToken cancellationToken)
        {
            var allFiles = await _fileRepository.GetAll() ?? new List<GiftApi.Domain.Entities.File>();

            var query = allFiles.Where(f => !f.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.FileName))
                query = query.Where(f => f.FileName != null && f.FileName.Contains(request.FileName, StringComparison.OrdinalIgnoreCase));

            if (request.FileType.HasValue)
                query = query.Where(f => f.FileType == request.FileType);

            if (request.UserId.HasValue)
                query = query.Where(f => f.UserId == request.UserId);

            if (request.MainImage.HasValue)
                query = query.Where(f => f.MainImage == request.MainImage);

            if (request.UploadDateFrom.HasValue)
                query = query.Where(f => f.UploadDate >= request.UploadDateFrom);

            if (request.UploadDateTo.HasValue)
                query = query.Where(f => f.UploadDate <= request.UploadDateTo);

            var page = request.Pagination.Page <= 0 ? 1 : request.Pagination.Page;
            var pageSize = request.Pagination.PageSize <= 0 ? 10 : request.Pagination.PageSize;

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(f => f.UploadDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new GetAllFilesItemsResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    UserId = f.UserId,
                    MainImage = f.MainImage,
                    UploadDate = f.UploadDate
                })
                .ToList();

            return new GetAllFilesResponse
            {
                Success = true,
                StatusCode = 200,
                TotalCount = totalCount,
                Items = items
            };
        }
    }
}