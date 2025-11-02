using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Create
{
    public class CreateBrandCommand : IRequest<CreateBrandResponse>
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public int CategoryId { get; set; }
    }
}
