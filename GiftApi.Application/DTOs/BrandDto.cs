namespace GiftApi.Application.DTOs
{
    public class BrandDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public int? CategoryId { get; set; }
    }
}
