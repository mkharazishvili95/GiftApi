using GiftApi.Application.Manage.Commands.EditCategory;
using GiftApi.Application.Manage.Commands.UpdateCategory;
using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.Upda
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
    {
        readonly ApplicationDbContext _db;
        public UpdateCategoryHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.Id <= 0)
                return new UpdateCategoryResponse { StatusCode = 400, Success = false, UserMessage = "CategoryId is required" };

            var category = await _db.Categories.FindAsync(request.Id);
            if (category == null)
                return new UpdateCategoryResponse { StatusCode = 404, Success = false, UserMessage = "Category not found" };
            if (string.IsNullOrWhiteSpace(request.Name))
                return new UpdateCategoryResponse { StatusCode = 400, Success = false, UserMessage = "Category name is required" };

            category.Name = request.Name;
            category.Description = request.Description;
            category.Logo = request.Logo;
            category.UpdateDate = DateTime.UtcNow.AddHours(4);

            _db.Categories.Update(category);
            await _db.SaveChangesAsync(cancellationToken);

            return new UpdateCategoryResponse { StatusCode = 200, Success = true, UserMessage = "Category updated successfully"};
        }
    }
}
