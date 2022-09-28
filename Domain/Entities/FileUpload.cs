using Microsoft.AspNetCore.Http;

namespace Domain.Entites;

public class FileUpload
{
    public IFormFile? QuoteImage { get; set; }
}
