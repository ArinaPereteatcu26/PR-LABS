
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using BooksAPI.Entities;


    namespace BooksAPI.Data
    {
        public class BookDbContext : DbContext
        {
            public BookDbContext(DbContextOptions<BookDbContext> options)
                : base(options)
            {
            }

            public DbSet<Book> Books { get; set; }
        }
    }
