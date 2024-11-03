
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using BooksAPI.Models;

namespace BooksAPI.Data
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