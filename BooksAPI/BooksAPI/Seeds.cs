using BooksAPI.Data;
using BooksAPI.Entities;
using Bogus;
using System.Collections.Generic;

namespace BooksAPI
{
    public class Seeds
    {
        public static void SeedData(BookDbContext dbContext)
        {
            if (dbContext.Books.Any()) return;

            var faker = new Faker();
            var books = new List<Book>();
            var random = new Random();

            for (var i = 0; i < 1000; i++)
            {
                books.Add(new Book
                {
                    Name = faker.Company.CompanyName(),
                    Price = Math.Round((decimal)(random.NextDouble() * (100 - 5) + 5), 2),
                    Currency = "USD",
                    Year = random.Next(1900, 2024),
                    Link = "https://www.example.com/" + faker.Lorem.Sentence().ToLower().Replace(" ", "-")
                });
            }

            dbContext.Books.AddRange(books);
            dbContext.SaveChanges();
        }
    }
}
