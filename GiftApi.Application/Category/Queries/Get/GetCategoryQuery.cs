using MediatR;

namespace GiftApi.Application.Category.Queries.Get
{
    public class GetCategoryQuery : IRequest<GetCategoryResponse>
    {
        public int Id { get; set; }
    }
}
