using MediatR;

namespace GiftApi.Application.Features.Category.Queries.Get
{
    public class GetCategoryQuery : IRequest<GetCategoryResponse>
    {
        public int Id { get; set; }
    }
}
