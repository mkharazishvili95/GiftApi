namespace GiftApi.Application.Category.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Logo { get; set; }
    }
}
