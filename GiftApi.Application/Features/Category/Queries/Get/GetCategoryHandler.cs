using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.Get
{
    public class GetCategoryHandler : IRequestHandler<GetCategoryQuery, GetCategoryResponse>
    {
        readonly ICategoryRepository _categoryRepository;
        public GetCategoryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<GetCategoryResponse> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new GetCategoryResponse { StatusCode = 400, Success = false, Message = "Category ID is required." };

            var category = await _categoryRepository.Get(request.Id);

            if (category == null || category.IsDeleted)
                return new GetCategoryResponse { StatusCode = 404, Success = false, Message = "Category not found." };

            var response = new GetCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Logo = category.Logo,
                CreateDate = category.CreateDate,
                UpdateDate = category.UpdateDate,
                StatusCode = 200,
                Success = true
            };

            return response;
        }
    }
}
