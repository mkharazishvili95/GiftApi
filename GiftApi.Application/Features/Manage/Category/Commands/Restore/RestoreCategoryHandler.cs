using GiftApi.Application.Interfaces;
using MediatR;

namespace GiftApi.Application.Features.Manage.Category.Commands.Restore
{
    public class RestoreCategoryHandler : IRequestHandler<RestoreCategoryCommand, RestoreCategoryResponse>
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICategoryRepository _repository;
        public RestoreCategoryHandler(IUnitOfWork unitOfWork, ICategoryRepository repository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<RestoreCategoryResponse> Handle(RestoreCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new RestoreCategoryResponse { StatusCode = 400, Success = false, Message = "CategoryId is required" };

            var category = await _repository.Get(request.Id);

            if (category == null)
                return new RestoreCategoryResponse { StatusCode = 404, Success = false, Message = "Category not found" };

            if (!category.IsDeleted)
                return new RestoreCategoryResponse { StatusCode = 400, Success = false, Message = "Category is not deleted" };

            await _repository.Restore(request.Id);
            await _unitOfWork.SaveChangesAsync();

            return new RestoreCategoryResponse { StatusCode = 200, Success = true, Message = "Category restored successfully" };
        }
    }
}
