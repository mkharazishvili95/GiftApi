using GiftApi.Core.Entities;
using GiftApi.Infrastructure.Data;
using MediatR;

namespace GiftApi.Application.Manage.Commands.CreateCategory
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
    {
        readonly ApplicationDbContext _db;
        public CreateCategoryHandler(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            if(string.IsNullOrWhiteSpace(request.Name))
                return new CreateCategoryResponse { Success = false, UserMessage = "Category name is required.", StatusCode = 400 };

            var category = new GiftApi.Core.Entities.Category
            {
                Name = request.Name,
                Description = request.Description,
                Logo = request.Logo,
                CreateDate = DateTime.UtcNow.AddHours(4),
                IsDeleted = false,
                DeleteDate = null, 
                UpdateDate = null  
            };

            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync(cancellationToken);

            return new CreateCategoryResponse { Success = true, UserMessage = "Category created successfully.", StatusCode = 201, Id = category.Id };
        }
    }
}
