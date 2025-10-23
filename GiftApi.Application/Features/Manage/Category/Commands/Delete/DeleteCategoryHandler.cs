using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Delete
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICategoryRepository _repository;
        public DeleteCategoryHandler(IUnitOfWork unitOfWork, ICategoryRepository repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new DeleteCategoryResponse { StatusCode = 400, Success = false, Message = "CategoryId is required" };

            var category = await _repository.Get(request.Id);

            if (category == null)
                return new DeleteCategoryResponse { StatusCode = 404, Success = false, Message = "Category not found" };

            if (category.IsDeleted)
                return new DeleteCategoryResponse { StatusCode = 400, Success = false, Message = "Category already deleted" };

            await _repository.Delete(request.Id);
            await _unitOfWork.SaveChangesAsync();

            return new DeleteCategoryResponse { StatusCode = 200, Success = true, Message = "Category deleted successfully" };
        }
    }
}
