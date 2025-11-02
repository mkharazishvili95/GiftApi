using GiftApi.Application.Common.Models;

namespace GiftApi.Application.Features.Manage.Brand.Commands.Create
{
    public class CreateBrandResponse : BaseResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
    }
}
