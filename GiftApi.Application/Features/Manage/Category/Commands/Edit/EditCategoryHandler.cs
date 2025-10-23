using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Edit
{
    public class EditCategoryHandler : IRequestHandler<EditCategoryCommand, EditCategoryResponse>
    {
        readonly ICategoryRepository _repository;
        readonly IUnitOfWork _unitOfWork;
        public EditCategoryHandler(ICategoryRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EditCategoryResponse> Handle(EditCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _repository.Get(request.Id);

            if (category == null)
                return new EditCategoryResponse { Success = false, Message = "Category not found.", StatusCode = 404 };

            if (category.IsDeleted)
                return new EditCategoryResponse { Success = false, Message = "Category is deleted.", StatusCode = 400 };

            category.Name = request.Name;
            category.Description = request.Description;
            category.Logo = request.Logo;
            category.UpdateDate = DateTime.UtcNow.AddHours(4);

            var updatedCategory = await _repository.Edit(category);
            await _unitOfWork.SaveChangesAsync();

            return new EditCategoryResponse
            {
                Success = true,
                StatusCode = 200,
                Message = "Category updated successfully",
                Id = updatedCategory.Id,
                Name = updatedCategory.Name,
                Description = updatedCategory.Description,
                Logo = updatedCategory.Logo
            };
        }
    }
}
