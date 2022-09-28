using Dapper;
using Domain.Dtos;
using Domain.Wrapper;
using Infrastructure.DataContext;
using Domain;
using Microsoft.AspNetCore.Hosting;

public class QuoteService
{
    private DataContext _context;
    private readonly IWebHostEnvironment _environment;

    public QuoteService(DataContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<Response<List<Quote>>> GetQuotes()
    {
        await using var connection = _context.CreateConnection();
        var sql = "select  * from quote";
        var result = await connection.QueryAsync<Quote>(sql);
        return new Response<List<Quote>>(result.ToList());
    }

    public async Task<Response<List<QuoteCategoryDto>>> GetQuoteWithCategory()
    {
        await using var connection = _context.CreateConnection();
        var sql =
            "select q.Id, q.author, q.quoteText, c.CategoryName  from quote as q join category as c on c.Id = q.id;";
        var result = await connection.QueryAsync<QuoteCategoryDto>(sql);
        return new Response<List<QuoteCategoryDto>>(result.ToList());
    }

      public async Task<Response<GetQuoteDto>> AddQuote(CreateQuoteDto quote)
    {
        try
        {
            using var connection = _context.CreateConnection();
        {
            var path  = Path.Combine(_environment.WebRootPath, "images", quote.QuoteImage.FileName);
            using var stream = new FileStream(path, FileMode.Create);
            await quote.QuoteImage.CopyToAsync(stream);

            var sql = $"insert into Quote (Author, QuoteText, QuoteImage, CategoryId) VALUES (@Author, @QuoteText, @QuoteImage, @CategoryId) returning Id";
            var response  = await connection.ExecuteScalarAsync<int>(sql, new{quote.Author, quote.QuoteText, QuoteImage = quote.QuoteImage.FileName, quote.CategoryId});
            quote.Id = response;
            var getQuote = new GetQuoteDto
            {
                Id = quote.Id,
                Author = quote.Author,
                QuoteText = quote.QuoteText,
                QuoteImage = quote.QuoteImage.FileName,
                CategoryId = quote.CategoryId
            };
            return new Response<GetQuoteDto>(getQuote);
        }
        }
        catch (Exception e)
        {
            
          return new Response<GetQuoteDto>(System.Net.HttpStatusCode.InternalServerError, e.Message);
        }   
}
}