using ClosedXML.Excel;
using InyarthaApp.Models;
using System.Text.Json;

namespace InyarthaApp.Services;

public class ExcelService
{
    private readonly string _filePath;
    private readonly string _jsonDir;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ExcelService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        _filePath = Path.Combine(dataDir, "quotes.xlsx");
        _jsonDir = Path.Combine(dataDir, "Quotes");
        Directory.CreateDirectory(dataDir);
        if (!File.Exists(_filePath)) CreateSheet();
    }

    private void CreateSheet()
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Quotes");
        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "ClientName";
        ws.Cell(1, 3).Value = "ClientPhone";
        ws.Cell(1, 4).Value = "ProjectType";
        ws.Cell(1, 5).Value = "Description";
        ws.Cell(1, 6).Value = "Status";
        ws.Cell(1, 7).Value = "CreatedAt";
        ws.Range("A1:G1").Style.Font.Bold = true;
        ws.Columns().AdjustToContents();
        wb.SaveAs(_filePath);
    }

    private XLWorkbook OpenRead()
    {
        var bytes = File.ReadAllBytes(_filePath);
        var ms = new MemoryStream(bytes);
        return new XLWorkbook(ms);
    }

    private void WriteBack(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        File.WriteAllBytes(_filePath, ms.ToArray());
    }

    public async Task<List<Quote>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return new List<Quote>();
            using var wb = OpenRead();
            var ws = wb.Worksheet("Quotes");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();

            if (rows == null || rows.Count == 0)
            {
                rows = TryMigrateFromJson(ws);
                if (rows != null && rows.Count > 0)
                    WriteBack(wb);
            }

            var list = new List<Quote>();
            if (rows != null)
            {
                foreach (var row in rows)
                {
                    list.Add(new Quote
                    {
                        Id = row.Cell(1).GetString(),
                        ClientName = row.Cell(2).GetString(),
                        ClientPhone = row.Cell(3).GetString(),
                        ProjectType = row.Cell(4).GetString(),
                        Scope = row.Cell(5).GetString(),
                        Status = row.Cell(6).GetString(),
                        CreatedAt = DateTime.TryParse(row.Cell(7).GetString(), out var dt) ? dt : DateTime.Now
                    });
                }
            }
            return list.OrderByDescending(q => q.CreatedAt).ToList();
        }
        finally { _lock.Release(); }
    }

    private List<IXLRangeRow>? TryMigrateFromJson(IXLWorksheet ws)
    {
        if (!Directory.Exists(_jsonDir)) return null;
        var migrated = false;
        foreach (var file in Directory.GetFiles(_jsonDir, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var q = JsonSerializer.Deserialize<Quote>(json);
                if (q == null) continue;
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                ws.Cell(lastRow + 1, 1).Value = q.Id;
                ws.Cell(lastRow + 1, 2).Value = q.ClientName;
                ws.Cell(lastRow + 1, 3).Value = q.ClientPhone ?? "";
                ws.Cell(lastRow + 1, 4).Value = q.ProjectType;
                ws.Cell(lastRow + 1, 5).Value = q.Scope ?? "";
                ws.Cell(lastRow + 1, 6).Value = q.Status;
                ws.Cell(lastRow + 1, 7).Value = q.CreatedAt.ToString("o");
                migrated = true;
            }
            catch { }
        }
        if (!migrated) return null;
        return ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
    }

    public async Task UpsertAsync(Quote quote)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) CreateSheet();
            using var wb = OpenRead();
            var ws = wb.Worksheet("Quotes");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            var existing = rows?.FirstOrDefault(r => r.Cell(1).GetString() == quote.Id);
            if (existing != null)
            {
                existing.Cell(2).Value = quote.ClientName;
                existing.Cell(3).Value = quote.ClientPhone ?? "";
                existing.Cell(4).Value = quote.ProjectType;
                existing.Cell(5).Value = quote.Scope ?? "";
                existing.Cell(6).Value = quote.Status;
                existing.Cell(7).Value = quote.CreatedAt.ToString("o");
            }
            else
            {
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                ws.Cell(lastRow + 1, 1).Value = quote.Id;
                ws.Cell(lastRow + 1, 2).Value = quote.ClientName;
                ws.Cell(lastRow + 1, 3).Value = quote.ClientPhone ?? "";
                ws.Cell(lastRow + 1, 4).Value = quote.ProjectType;
                ws.Cell(lastRow + 1, 5).Value = quote.Scope ?? "";
                ws.Cell(lastRow + 1, 6).Value = quote.Status;
                ws.Cell(lastRow + 1, 7).Value = quote.CreatedAt.ToString("o");
            }
            WriteBack(wb);
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return;
            using var wb = OpenRead();
            var ws = wb.Worksheet("Quotes");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            var row = rows?.FirstOrDefault(r => r.Cell(1).GetString() == id);
            if (row != null)
            {
                row.Delete();
                WriteBack(wb);
            }
        }
        finally { _lock.Release(); }
    }
}
