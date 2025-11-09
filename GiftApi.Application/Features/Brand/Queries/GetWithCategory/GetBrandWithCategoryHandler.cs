using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Brand.Queries.GetWithCategory
{
    public class GetBrandWithCategoryHandler : IRequestHandler<GetBrandWithCategoryQuery, GetBrandWithCategoryResponse>
    {
        readonly IBrandRepository _brandRepository;
        public GetBrandWithCategoryHandler(IBrandRepository brandRepository)
        {
            _brandRepository = brandRepository;
        }

        public async Task<GetBrandWithCategoryResponse> Handle(GetBrandWithCategoryQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new GetBrandWithCategoryResponse { StatusCode = 400, Success = false, Message = "Brand ID is required." };

            var brand = await _brandRepository.GetWithCategory(request.Id);

            if (brand == null)
                return new GetBrandWithCategoryResponse { StatusCode = 404, Success = false, Message = "Brand not found." };

            var response = new GetBrandWithCategoryResponse
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                CreateDate = brand.CreateDate,
                UpdateDate = brand.UpdateDate,
                CategoryId = brand.CategoryId,
                Category = brand.Category != null ? new Application.DTOs.CategoryDto
                {
                    Id = brand.Category.Id,
                    Name = brand.Category.Name,
                    Description = brand.Category.Description,
                    CreateDate = brand.Category.CreateDate,
                    UpdateDate = brand.Category.UpdateDate
                } : null,
                StatusCode = 200,
                Success = true
            };

            return response;
        }
    }
}
