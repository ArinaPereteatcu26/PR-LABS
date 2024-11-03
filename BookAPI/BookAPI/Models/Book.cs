using System.ComponentModel.DataAnnotations;

namespace BookAPI.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? Currency { get; set; }

        public int Year { get; set; }

        [Required]
        public string Link { get; set; }
    }
}
