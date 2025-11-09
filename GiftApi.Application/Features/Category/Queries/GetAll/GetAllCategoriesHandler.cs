using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Category.Queries.GetAll
{
    public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, GetAllCategoriesResponse>
    {
        readonly ICategoryRepository _categoryRepository;
        public GetAllCategoriesHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<GetAllCategoriesResponse> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAll(cancellationToken);

            if (categories == null || !categories.Any())
            {
                return new GetAllCategoriesResponse
                {
                    Items = new List<GetAllCategoriesItemsResponse>(),
                    TotalCount = 0,
                    Success = true,
                    StatusCode = 200
                };
            }
            var totalCount = categories.Count();

            var items = categories
                    .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                    .Take(request.Pagination.PageSize)
                    .Select(c => new GetAllCategoriesItemsResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        CreateDate = c.CreateDate,
                        UpdateDate = c.UpdateDate,
                        Logo = c.Logo
                    })
                    .ToList();

            return new GetAllCategoriesResponse
            {
                Items = items,
                TotalCount = totalCount,
                Success = true,
                StatusCode = 200
            };
        }
    }
}
