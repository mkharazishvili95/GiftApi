using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetAll
{
    public class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, GetAllBrandsResponse>
    {
        readonly IBrandRepository _brandRepository;

        public GetAllBrandsHandler(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<GetAllBrandsResponse> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.GetAllBrandsAsync(cancellationToken);

            if (brands == null || !brands.Any())
            {
                return new GetAllBrandsResponse
                {
                    Items = new List<GetAllBrandsItemsResponse>(),
                    TotalCount = 0,
                    Success = true,
                    StatusCode = 200
                };
            }
            var items = brands
                    .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(b => new GetAllBrandsItemsResponse
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        LogoUrl = b.LogoUrl,
                        Website = b.Website,
                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        CategoryId = b.CategoryId
                    })
                    .ToList();

            return new GetAllBrandsResponse
            {
                Items = items,
                TotalCount = brands.Count,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
