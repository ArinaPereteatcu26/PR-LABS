using BooksAPI.Data;
using BooksAPI.Entities;

using Microsoft.EntityFrameworkCore;

namespace BooksAPI.Repositories;

public class BookRepository(BookDbContext context)
{
    public BookDbContext GetContext()
    {
        return context;
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(int offset, int limit)
    {
        return await context.Books
            .Skip(offset)
            .Take(limit)
            .OrderBy(book => book.Name)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await context.Books.FindAsync(id);
    }

    public async Task<Book> AddBookAsync(Book book)
    {
        context.Books.Add(book);
        await context.SaveChangesAsync();
        return book;
    }

    public async Task<bool> UpdateBookAsync(Book book)
    {
        context.Entry(book).State = EntityState.Modified;
        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null) return false;

        context.Books.Remove(book);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddBooksAsync(IEnumerable<Book> books)
    {
        await context.Books.AddRangeAsync(books);
        await context.SaveChangesAsync();
        return true;
    }

    public bool BookExists(int id)
    {
        return context.Books.Any(e => e.Id == id);
    }
}