using System.Text.Json;
using InyarthaApp.Models;

namespace InyarthaApp.Services;

public interface IQuoteService
{
    Task<List<Quote>> GetAllAsync();
    Task<Quote?> GetByIdAsync(string id);
    Task SaveAsync(Quote quote);
    Task DeleteAsync(string id);
}

public class QuoteService : IQuoteService
{
    private readonly string _folder;

    public QuoteService(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.ContentRootPath, "Data", "Quotes");
        Directory.CreateDirectory(_folder);
    }

    private string FilePath(string id) => Path.Combine(_folder, $"{id}.json");

    public async Task<List<Quote>> GetAllAsync()
    {
        var list = new List<Quote>();
        foreach (var file in Directory.GetFiles(_folder, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var q = JsonSerializer.Deserialize<Quote>(json);
            if (q != null) list.Add(q);
        }
        return list.OrderByDescending(q => q.CreatedAt).ToList();
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
    }

    public async Task DeleteAsync(string id)
    {
        var path = FilePath(id);
        if (File.Exists(path)) File.Delete(path);
        await Task.CompletedTask;
    }
}