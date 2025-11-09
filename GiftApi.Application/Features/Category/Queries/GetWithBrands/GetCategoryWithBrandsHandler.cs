using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetWithBrands
{
    public class GetCategoryWithBrandsHandler : IRequestHandler<GetCategoryWithBrandsQuery, GetCategoryWithBrandsResponse>
    {
        readonly ICategoryRepository _categoryRepository;
        public GetCategoryWithBrandsHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<GetCategoryWithBrandsResponse> Handle(GetCategoryWithBrandsQuery request, CancellationToken cancellationToken)
        {
            if (request.CategoryId <= 0)
                return new GetCategoryWithBrandsResponse { StatusCode = 400, Success = false, Message = "Category ID is required." };

            var category = await _categoryRepository.GetWithBrands(request.CategoryId);

            if (category == null || category.IsDeleted)
                return new GetCategoryWithBrandsResponse { StatusCode = 404, Success = false, Message = "Category not found." };

            var response = new GetCategoryWithBrandsResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Logo = category.Logo,
                CreateDate = category.CreateDate,
                UpdateDate = category.UpdateDate,
                Brands = category.Brands.Select(b => new DTOs.BrandDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    LogoUrl = b.LogoUrl,
                    Website = b.Website,
                }).ToList(),
                StatusCode = 200,
                Success = true
            };

            return response;
        }
    }
}
