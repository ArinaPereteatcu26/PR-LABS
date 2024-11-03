using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BooksAPI.Data;
using BooksAPI.Models;



[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBooks(int offset = 0, int limit = 5)
    {
        var totalBooks = await _context.Books.CountAsync();
        var books = await _context.Books
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        var response = new
        {
            TotalCount = totalBooks,
            Offset = offset,
            Limit = limit,
            Books = books
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, [FromBody] Book updatedBook)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        book.Name = updatedBook.Name;
        book.Price = updatedBook.Price;
        book.Year = updatedBook.Year;
        book.Link = updatedBook.Link;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.FileName);

        var directoryPath = Path.GetDirectoryName(filePath);


        if (directoryPath != null)
        {
            Directory.CreateDirectory(directoryPath);
        }
        else
        {
            return BadRequest("Invalid file path.");
        }

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
        var books = JsonSerializer.Deserialize<List<Book>>(jsonContent);

        if (books != null)
        {
            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();
        }

        return Ok(new { FilePath = filePath, FileName = file.FileName });
    }



}