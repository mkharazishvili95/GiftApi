using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.RestoreCategory
{
    public class RestoreCategoryHandler : IRequestHandler<RestoreCategoryCommand, RestoreCategoryResponse>
    {
        readonly ApplicationDbContext _db;
        public RestoreCategoryHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<RestoreCategoryResponse> Handle(RestoreCategoryCommand request, CancellationToken cancellationToken)
        {
            if(request.Id <= 0)
                return new RestoreCategoryResponse { StatusCode = 400, Success = false, UserMessage = "CategoryId is required" };

            var category = await _db.Categories.FindAsync(request.Id);
            if(category == null)
                return new RestoreCategoryResponse { StatusCode = 404, Success = false, UserMessage = "Category not found" };

            if(!category.IsDeleted)
                return new RestoreCategoryResponse { StatusCode = 400, Success = false, UserMessage = "Category is not deleted" };

            category.IsDeleted = false;
            category.DeleteDate = null;
            category.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(category);
            await _db.SaveChangesAsync(cancellationToken);

            return new RestoreCategoryResponse { StatusCode = 200, Success = true, UserMessage = "Category restored successfully" };
        }
    }
}
