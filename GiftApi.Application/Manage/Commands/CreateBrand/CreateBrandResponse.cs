using GiftApi.Application.Common.Responses;

namespace GiftApi.Application.Manage.Commands.CreateBrand
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
