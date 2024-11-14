using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BooksAPI.Entities;
[Table("books")]
public class Book
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public string? Name { get; set; }

    [Required]
    [Column("price")]
    public decimal Price { get; set; }

    [Column("currency")]
    public string? Currency { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Required]
    [Column("link")]
    public string? Link { get; set; }
}