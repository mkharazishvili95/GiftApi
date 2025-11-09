using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetAllWithCategories
{
    public class GetAllBrandsWithCategoriesHandler : IRequestHandler<GetAllBrandsWithCategoriesQuery, GetAllBrandsWithCategoriesResponse>
    {
        readonly IBrandRepository _brandRepository;
        public GetAllBrandsWithCategoriesHandler(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<GetAllBrandsWithCategoriesResponse> Handle(GetAllBrandsWithCategoriesQuery request, CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.GetAllBrandsWithCategoriesAsync(cancellationToken);

            if (brands == null || !brands.Any())
            {
                return new GetAllBrandsWithCategoriesResponse
                {
                    Items = new List<GetAllBrandsWithCategoriesItemsResponse>(),
                    TotalCount = 0,
                    Success = true,
                    StatusCode = 200
                };
            }
            var totalCount = brands.Count();

            var items = brands
                    .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(b => new GetAllBrandsWithCategoriesItemsResponse
                    {
                        Id = b.Id,
                        Name = b.Name,
                        Description = b.Description,
                        LogoUrl = b.LogoUrl,
                        Website = b.Website,
                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        CategoryId = b.CategoryId,
                        Category = b.Category != null ? new Application.DTOs.CategoryDto
                        {
                            Id = b.Category.Id,
                            Name = b.Category.Name,
                            Description = b.Category.Description,
                            CreateDate = b.Category.CreateDate,
                            UpdateDate = b.Category.UpdateDate
                        } : null
                    })
                    .ToList();

            return new GetAllBrandsWithCategoriesResponse
            {
                Items = items,
                TotalCount = totalCount,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
