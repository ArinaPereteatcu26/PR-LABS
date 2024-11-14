using System.Text.Json;
using BooksAPI.Entities;
using BooksAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BooksAPI.Controllers;

[ApiController]
[Route("api")]
public class BooksController(BookRepository repository) : ControllerBase
{
    [HttpGet("books")]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks([FromQuery] int offset = 0, [FromQuery] int limit = 5)
    {
        return Ok(await repository.GetBooksAsync(offset, limit));
    }

    [HttpGet("book/{id:int}")]
    public async Task<ActionResult<Book>> GetBook(int id)
    {
        var book = await repository.GetBookByIdAsync(id);
        if (book == null) return NotFound();
        return book;
    }

    [HttpPost("add")]
    public async Task<ActionResult<Book>> PostBook(Book book)
    {
        var createdBook = await repository.AddBookAsync(book);
        return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
    }

    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.Id) return BadRequest("ID in URL does not match ID in body");
        if (await repository.UpdateBookAsync(book)) return NoContent();
        if (!repository.BookExists(id)) return NotFound();

        throw new DbUpdateConcurrencyException();
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        if (!await repository.DeleteBookAsync(id)) return NotFound();

        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");

        using var streamReader = new StreamReader(file.OpenReadStream());
        var content = await streamReader.ReadToEndAsync();

        try
        {
            var books = JsonSerializer.Deserialize<List<Book>>(content);
            if (books != null)
            {
                await repository.AddBooksAsync(books);
                return Ok($"Successfully imported {books.Count} books");
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Error processing file: {ex.Message}");
        }

        return Ok("File uploaded");
    }

    [HttpGet("seed")]
    public ActionResult Seed()
    {
        Seeds.SeedData(repository.GetContext());
        return Ok("Seeded.");
    }
}