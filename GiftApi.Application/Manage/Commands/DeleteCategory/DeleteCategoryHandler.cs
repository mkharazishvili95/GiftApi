using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.DeleteCategory
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
    {
        readonly ApplicationDbContext _db;
        public DeleteCategoryHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new DeleteCategoryResponse { StatusCode = 400, Success = false, UserMessage = "CategoryId is required" };

            var category = await _db.Categories.FindAsync(request.Id);

            if(category == null)
                return new DeleteCategoryResponse { StatusCode = 404, Success = false, UserMessage = "Category not found" };

            if(category.IsDeleted)
                return new DeleteCategoryResponse { StatusCode = 400, Success = false, UserMessage = "Category is already deleted" };

            category.IsDeleted = true;
            category.DeleteDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(category);
            await _db.SaveChangesAsync(cancellationToken);

            return new DeleteCategoryResponse { StatusCode = 200, Success = true, UserMessage = "Category deleted successfully" };
        }
    }
}
