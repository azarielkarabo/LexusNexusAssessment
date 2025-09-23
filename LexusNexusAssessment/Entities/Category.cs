using System.ComponentModel.DataAnnotations;

namespace LexusNexusAssessment.Models
{
    public class Category : IComparable<Category>
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public DateTime CreatedAt { get; set; }


        // Navigation properties
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public Category()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public int CompareTo(Category? other)
        {
            throw new NotImplementedException();
        }
    }
}