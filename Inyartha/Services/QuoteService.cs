using System.Text.Json;
using InyarthaApp.Models;

namespace InyarthaApp.Services;

public interface IQuoteService
{
    Task<List<Quote>> GetAllAsync();
    Task<Quote?> GetByIdAsync(string id);
    Task SaveAsync(Quote quote);
    Task DeleteAsync(string id);
    Task<int> GeneratePdfsAsync(string[] ids);
}

public class QuoteService : IQuoteService
{
    private readonly string _folder;
    private readonly ExcelService _excel;
    private readonly PdfService _pdf;

    public QuoteService(IWebHostEnvironment env, ExcelService excel, PdfService pdf)
    {
        _folder = Path.Combine(env.ContentRootPath, "Data", "Quotes");
        _excel = excel;
        _pdf = pdf;
        Directory.CreateDirectory(_folder);
    }

    private string FilePath(string id) => Path.Combine(_folder, $"{id}.json");

    public async Task<List<Quote>> GetAllAsync()
    {
        return await _excel.GetAllAsync();
    }

    public async Task<Quote?> GetByIdAsync(string id)
    {
        var path = FilePath(id);
        if (!File.Exists(path)) return null;
        return JsonSerializer.Deserialize<Quote>(await File.ReadAllTextAsync(path));
    }

    public async Task SaveAsync(Quote quote)
    {
        var json = JsonSerializer.Serialize(quote,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(FilePath(quote.Id), json);

        await _excel.UpsertAsync(quote);

        await _pdf.DeleteAsync(quote.Id);

        await _pdf.GenerateAsync(quote);
    }

    public async Task<int> GeneratePdfsAsync(string[] ids)
    {
        var count = 0;
        foreach (var id in ids)
        {
            var q = await GetByIdAsync(id);
            if (q == null) continue;
            try
            {
                await _pdf.GenerateAsync(q);
                count++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[QuoteService] PDF generation failed for {id}: {ex.Message}");
            }
        }
        return count;
    }

    public async Task DeleteAsync(string id)
    {
        var path = FilePath(id);
        if (File.Exists(path)) File.Delete(path);

        await _excel.DeleteAsync(id);
        await _pdf.DeleteAsync(id);
    }
}
