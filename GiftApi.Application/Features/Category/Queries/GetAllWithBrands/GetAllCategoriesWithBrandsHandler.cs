using GiftApi.Application.DTOs;
using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetAllWithBrands
{
    public class GetAllCategoriesWithBrandsHandler : IRequestHandler<GetAllCategoriesWithBrandsQuery, GetAllCategoriesWithBrandsResponse>
    {
        readonly ICategoryRepository _categoryRepository;
        public GetAllCategoriesWithBrandsHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<GetAllCategoriesWithBrandsResponse> Handle(GetAllCategoriesWithBrandsQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllCategoriesWithBrandsAsync(cancellationToken);

            if (categories == null || !categories.Any())
            {
                return new GetAllCategoriesWithBrandsResponse
                {
                    Items = new List<GetAllCategoriesWithBrandsItemsResponse>(),
                    TotalCount = 0,
                    Success = true,
                    StatusCode = 200
                };
            }
            var totalCount = categories.Count();

            var items = categories
                    .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(c => new GetAllCategoriesWithBrandsItemsResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        CreateDate = c.CreateDate,
                        UpdateDate = c.UpdateDate,
                        Logo = c.Logo,
                        Brands = c.Brands.Select(b => new BrandDto
                        {
                            Id = b.Id,
                            Name = b.Name,
                            Description = b.Description,
                            Website = b.Website,
                            LogoUrl = b.LogoUrl

                        }).ToList()
                    })
                    .ToList();

            return new GetAllCategoriesWithBrandsResponse
            {
                Items = items,
                TotalCount = totalCount,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
