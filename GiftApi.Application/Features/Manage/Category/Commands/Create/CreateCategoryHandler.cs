using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Create
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
    {
        readonly ICategoryRepository _repository;
        readonly IUnitOfWork _unitOfWork;
        public CreateCategoryHandler(ICategoryRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return new CreateCategoryResponse { Success = false, Message = "Category name is required.", StatusCode = 400 };

            var category = new GiftApi.Domain.Entities.Category
            {
                Name = request.Name,
                Description = request.Description,
                Logo = request.Logo,
                CreateDate = DateTime.UtcNow.AddHours(4),
                IsDeleted = false,
                DeleteDate = null,
                UpdateDate = null
            };

            await _repository.Create(category);
            await _unitOfWork.SaveChangesAsync();

            return new CreateCategoryResponse { Success = true, Message = "Category created successfully.", StatusCode = 201, Id = category.Id };
        }
    }
}
