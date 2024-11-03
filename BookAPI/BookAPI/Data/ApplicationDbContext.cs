using BookAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().ToTable("Books");

            modelBuilder.Entity<Book>().HasIndex(b => b.Name).HasDatabaseName("idx_books_name");
            modelBuilder.Entity<Book>().HasIndex(b => b.Year).HasDatabaseName("idx_books_year");
        }
    }
}
