using ClosedXML.Excel;
using InyarthaApp.Models;

namespace InyarthaApp.Services;

public class ItemsService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private const int ColName = 1;
    private const int ColMaterial = 2;
    private const int ColDescription = 3;
    private const int ColRate = 4;
    private const int ColPricingUnit = 5;

    public ItemsService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data", "Quotes");
        _filePath = Path.Combine(dataDir, "items.xlsx");
        Directory.CreateDirectory(dataDir);
    }

    private void EnsureHeader(IXLWorksheet ws)
    {
        ws.Cell(1, ColName).Value = "Item Name";
        ws.Cell(1, ColMaterial).Value = "Material";
        ws.Cell(1, ColDescription).Value = "Description";
        ws.Cell(1, ColRate).Value = "Rate";
        ws.Cell(1, ColPricingUnit).Value = "Pricing Unit";
        ws.Range("A1:E1").Style.Font.Bold = true;
    }

    public async Task SeedIfNotExistsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (File.Exists(_filePath)) return;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Items");
            EnsureHeader(ws);

            var items = GetSeedData();
            for (int i = 0; i < items.Count; i++)
            {
                WriteRow(ws, i + 2, items[i]);
            }
            ws.Columns().AdjustToContents();
            SaveWorkbook(wb);
        }
        finally { _lock.Release(); }
    }

    private static void WriteRow(IXLWorksheet ws, int rowNum, ItemTemplate item)
    {
        ws.Cell(rowNum, ColName).Value = item.Name;
        ws.Cell(rowNum, ColMaterial).Value = item.Material ?? "";
        ws.Cell(rowNum, ColDescription).Value = item.Description ?? "";
        ws.Cell(rowNum, ColRate).Value = (double)item.Rate;
        ws.Cell(rowNum, ColPricingUnit).Value = item.PricingUnit ?? "Per sq ft";
    }

    private static ItemTemplate ReadRow(IXLRangeRow row)
    {
        return new ItemTemplate
        {
            Name = row.Cell(ColName).GetString().Trim(),
            Material = row.Cell(ColMaterial).GetString().Trim(),
            Description = row.Cell(ColDescription).GetString().Trim(),
            Rate = (decimal)row.Cell(ColRate).GetDouble(),
            PricingUnit = row.Cell(ColPricingUnit).GetString().Trim()
        };
    }

    private XLWorkbook OpenWorkbook()
    {
        var bytes = File.ReadAllBytes(_filePath);
        var ms = new MemoryStream(bytes);
        return new XLWorkbook(ms);
    }

    private void SaveWorkbook(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        File.WriteAllBytes(_filePath, ms.ToArray());
    }

    public async Task AddAsync(ItemTemplate item)
    {
        await _lock.WaitAsync();
        try
        {
            var wb = File.Exists(_filePath)
                ? OpenWorkbook()
                : new XLWorkbook();
            using (wb)
            {
                var ws = wb.Worksheets.Count == 0
                    ? wb.Worksheets.Add("Items")
                    : wb.Worksheet("Items");

                if (ws.Cell(1, ColName).Value.ToString() != "Item Name")
                    EnsureHeader(ws);

                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                WriteRow(ws, lastRow + 1, item);
                ws.Columns().AdjustToContents();
                SaveWorkbook(wb);
            }
        }
        finally { _lock.Release(); }
    }

    public async Task UpdateAsync(string originalName, string originalMaterial, ItemTemplate item)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return;
            using var wb = OpenWorkbook();
            var ws = wb.Worksheet("Items");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            if (rows == null) return;

            var target = rows.FirstOrDefault(r =>
                r.Cell(ColName).GetString().Trim().Equals(originalName, StringComparison.OrdinalIgnoreCase) &&
                r.Cell(ColMaterial).GetString().Trim().Equals(originalMaterial, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                WriteRow(ws, target.RowNumber(), item);
                ws.Columns().AdjustToContents();
                SaveWorkbook(wb);
            }
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(string name, string material)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return;
            using var wb = OpenWorkbook();
            var ws = wb.Worksheet("Items");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            if (rows == null) return;

            var target = rows.FirstOrDefault(r =>
                r.Cell(ColName).GetString().Trim().Equals(name, StringComparison.OrdinalIgnoreCase) &&
                r.Cell(ColMaterial).GetString().Trim().Equals(material, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                target.Delete();
                ws.Columns().AdjustToContents();
                SaveWorkbook(wb);
            }
        }
        finally { _lock.Release(); }
    }

    public async Task CopyAsync(string name, string material)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return;
            using var wb = OpenWorkbook();
            var ws = wb.Worksheet("Items");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1).ToList();
            if (rows == null) return;

            var target = rows.FirstOrDefault(r =>
                r.Cell(ColName).GetString().Trim().Equals(name, StringComparison.OrdinalIgnoreCase) &&
                r.Cell(ColMaterial).GetString().Trim().Equals(material, StringComparison.OrdinalIgnoreCase));

            if (target != null)
            {
                var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                var dup = new ItemTemplate
                {
                    Name = target.Cell(ColName).GetString() + " (Copy)",
                    Material = target.Cell(ColMaterial).GetString(),
                    Description = target.Cell(ColDescription).GetString(),
                    Rate = (decimal)target.Cell(ColRate).GetDouble(),
                    PricingUnit = target.Cell(ColPricingUnit).GetString()
                };
                WriteRow(ws, lastRow + 1, dup);
                ws.Columns().AdjustToContents();
                SaveWorkbook(wb);
            }
        }
        finally { _lock.Release(); }
    }

    public async Task<ItemTemplate?> GetByNameMaterialAsync(string name, string material)
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return null;
            using var wb = OpenWorkbook();
            var ws = wb.Worksheet("Items");
            var rows = ws.RangeUsed()?.RowsUsed().Skip(1);
            if (rows == null) return null;

            foreach (var row in rows)
            {
                var rowName = row.Cell(ColName).GetString().Trim();
                var rowMat = row.Cell(ColMaterial).GetString().Trim();
                if (rowName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                    rowMat.Equals(material, StringComparison.OrdinalIgnoreCase))
                {
                    return ReadRow(row);
                }
            }
            return null;
        }
        finally { _lock.Release(); }
    }

    public async Task<List<ItemTemplate>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath)) return new List<ItemTemplate>();
            var bytes = await File.ReadAllBytesAsync(_filePath);
            using var ms = new MemoryStream(bytes);
            using var wb = new XLWorkbook(ms);
            var ws = wb.Worksheet("Items");
            var rows = ws.RangeUsed().RowsUsed().Skip(1);
            var list = new List<ItemTemplate>();
            foreach (var row in rows)
            {
                var name = row.Cell(ColName).GetString().Trim();
                if (string.IsNullOrEmpty(name)) continue;
                list.Add(ReadRow(row));
            }
            return list;
        }
        finally { _lock.Release(); }
    }

    private static List<ItemTemplate> GetSeedData()
    {
        return new List<ItemTemplate>
        {
            new() { Name = "Kitchen Base", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Middle Unit", Material = "BWP Plywood", Description = "", Rate = 1500, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Loft", Material = "BWP Plywood", Description = "", Rate = 1200, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Breakfast Table", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Tall Unit", Material = "BWP Plywood", Description = "Contains: Pantry Tall Unit | Electrical Appliances Tall Unit", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Pantry", Material = "", Description = "", Rate = 30000, PricingUnit = "Lump sum" },
            new() { Name = "Kitchen Utility Base", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Janitor Unit", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Tiles", Material = "Moroccan Tiles", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Granite", Material = "", Description = "", Rate = 900, PricingUnit = "Per sq ft" },
            new() { Name = "Kitchen Glass Door", Material = "", Description = "Tinted Glass Door - Saint Gobain", Rate = 5000, PricingUnit = "Lump sum" },
            new() { Name = "Kitchen Tandem", Material = "Ebco Livsmart Tandems and Bottle Pullout", Description = "", Rate = 4000, PricingUnit = "Lump sum" },
            new() { Name = "Kitchen Panel", Material = "BWP Plywood", Description = "", Rate = 800, PricingUnit = "Per sq ft" },
            new() { Name = "Foyer", Material = "MR Plywood", Description = "", Rate = 1300, PricingUnit = "Per sq ft" },
            new() { Name = "Foyer Mirror", Material = "", Description = "Saint Gobain Mirror", Rate = 5000, PricingUnit = "Lump sum" },
            new() { Name = "Outside Shoe Rack", Material = "MR Plywood", Description = "Paint: DUCO", Rate = 1300, PricingUnit = "Per sq ft" },
            new() { Name = "Living Seating", Material = "MR Plywood", Description = "Paint: DUCO", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Pooja Mandir", Material = "MR Plywood", Description = "CNC Cutting and Design | Paint: DUCO", Rate = 1900, PricingUnit = "Per sq ft" },
            new() { Name = "Crockery Unit", Material = "MR Plywood", Description = "", Rate = 1200, PricingUnit = "Per sq ft" },
            new() { Name = "Crockery Unit: Glass Door", Material = "", Description = "Saint Gobain Glasses", Rate = 4000, PricingUnit = "Lump sum" },
            new() { Name = "Wall Cover Unit", Material = "Gypsum Cover", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Room 1: Wardrobe", Material = "MR Plywood", Description = "", Rate = 1300, PricingUnit = "Per sq ft" },
            new() { Name = "Room 1: Loft", Material = "MR Plywood", Description = "", Rate = 900, PricingUnit = "Per sq ft" },
            new() { Name = "Room 1: Curved Table", Material = "MR Plywood", Description = "MS Rod Support", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Room 1: False Ceiling", Material = "Gypsum", Description = "", Rate = 70, PricingUnit = "Per sq ft" },
            new() { Name = "Vanity Unit", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Bathroom Storage", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Bathroom Floor Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Bathroom Wall Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Bathroom Upper Cover", Material = "Knauf Channel", Description = "PVC Sheets", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Room 1 Bathroom Mirror", Material = "", Description = "", Rate = 4000, PricingUnit = "Lump sum" },
            new() { Name = "MBR Wardrobe", Material = "MR Plywood", Description = "", Rate = 1300, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Loft", Material = "MR Plywood", Description = "", Rate = 900, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Dressing Unit", Material = "MR Plywood", Description = "Saint Gobain Mirror with AL Frame", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Panel", Material = "", Description = "Gyproc Panel", Rate = 300, PricingUnit = "Per sq ft" },
            new() { Name = "MBR False Ceiling", Material = "", Description = "Gyproc", Rate = 70, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Vanity", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Storage", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Floor Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Wall Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Upper Bathroom Covering", Material = "Knauf Channel", Description = "PVC Ceiling", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Bed Back", Material = "", Description = "", Rate = 400, PricingUnit = "Per sq ft" },
            new() { Name = "MBR Bathroom Mirror", Material = "", Description = "", Rate = 4000, PricingUnit = "Lump sum" },
            new() { Name = "MBR Bathroom Glass Separator", Material = "", Description = "", Rate = 900, PricingUnit = "Lump sum" },
            new() { Name = "Common Bathroom Floor Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Common Bathroom Wall Tiles", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Common Bathroom Upper Covering", Material = "", Description = "", Rate = 500, PricingUnit = "Per sq ft" },
            new() { Name = "Common Bathroom Vanity", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Common Bathroom Storage", Material = "BWP Plywood", Description = "", Rate = 1600, PricingUnit = "Per sq ft" },
            new() { Name = "Common Bathroom Mirror", Material = "", Description = "", Rate = 4000, PricingUnit = "Lump sum" },
            new() { Name = "TV Unit", Material = "", Description = "", Rate = 800, PricingUnit = "Per sq ft" },
            new() { Name = "Living Room False Ceiling", Material = "", Description = "Gyproc", Rate = 70, PricingUnit = "Per sq ft" },
            new() { Name = "Design Cost", Material = "", Description = "3D Drawing | 2D Drawing - Complete Internal Layout | Working Drawing", Rate = 50000, PricingUnit = "Lump sum" }
        };
    }
}
