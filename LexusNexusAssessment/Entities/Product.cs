using LexusNexusAssessment.Repositories.Base;
using System.ComponentModel.DataAnnotations;

namespace LexusNexusAssessment.Models
{
    public class Product : IComparable<Product>, IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? SKU { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        public int? CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual Category? Category { get; set; }

        public Product()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public int CompareTo(Product? other)
        {
            throw new NotImplementedException();
        }
    }
}