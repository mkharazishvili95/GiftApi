using MediatR;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Edit
{
    public class EditBrandCommand : IRequest<EditBrandResponse>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public int? CategoryId { get; set; }
    }
}
