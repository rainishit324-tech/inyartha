using ClosedXML.Excel;
using InyarthaApp.Models;

namespace InyarthaApp.Services;

public class ItemsService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ItemsService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data", "Quotes");
        _filePath = Path.Combine(dataDir, "items.xlsx");
        Directory.CreateDirectory(dataDir);
    }

    public async Task SeedIfNotExistsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (File.Exists(_filePath)) return;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Items");
            ws.Cell(1, 1).Value = "Item Name";
            ws.Cell(1, 2).Value = "Material";
            ws.Cell(1, 3).Value = "Description";
            ws.Cell(1, 4).Value = "Rate";
            ws.Range("A1:D1").Style.Font.Bold = true;

            var items = GetSeedData();
            for (int i = 0; i < items.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = items[i].Name;
                ws.Cell(i + 2, 2).Value = items[i].Material;
                ws.Cell(i + 2, 3).Value = items[i].Description;
                ws.Cell(i + 2, 4).Value = (double)items[i].Rate;
            }
            ws.Columns().AdjustToContents();
            wb.SaveAs(_filePath);
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
                var name = row.Cell(1).GetString().Trim();
                if (string.IsNullOrEmpty(name)) continue;
                list.Add(new ItemTemplate
                {
                    Name = name,
                    Material = row.Cell(2).GetString().Trim(),
                    Description = row.Cell(3).GetString().Trim(),
                    Rate = (decimal)row.Cell(4).GetDouble()
                });
            }
            return list;
        }
        finally { _lock.Release(); }
    }

    private static List<ItemTemplate> GetSeedData()
    {
        return new List<ItemTemplate>
        {
            new() { Name = "Kitchen Base", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Kitchen Middle Unit", Material = "BWP Plywood", Description = "", Rate = 1500 },
            new() { Name = "Kitchen Loft", Material = "BWP Plywood", Description = "", Rate = 1200 },
            new() { Name = "Kitchen Breakfast Table", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Kitchen Tall Unit", Material = "BWP Plywood", Description = "Contains: Pantry Tall Unit | Electrical Appliances Tall Unit", Rate = 1600 },
            new() { Name = "Kitchen Pantry", Material = "", Description = "", Rate = 30000 },
            new() { Name = "Kitchen Utility Base", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Kitchen Janitor Unit", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Kitchen Tiles", Material = "Moroccan Tiles", Description = "", Rate = 500 },
            new() { Name = "Kitchen Granite", Material = "", Description = "", Rate = 900 },
            new() { Name = "Kitchen Glass Door", Material = "", Description = "Tinted Glass Door - Saint Gobain", Rate = 5000 },
            new() { Name = "Kitchen Tandem", Material = "Ebco Livsmart Tandems and Bottle Pullout", Description = "", Rate = 4000 },
            new() { Name = "Kitchen Panel", Material = "BWP Plywood", Description = "", Rate = 800 },
            new() { Name = "Foyer", Material = "MR Plywood", Description = "", Rate = 1300 },
            new() { Name = "Foyer Mirror", Material = "", Description = "Saint Gobain Mirror", Rate = 5000 },
            new() { Name = "Outside Shoe Rack", Material = "MR Plywood", Description = "Paint: DUCO", Rate = 1300 },
            new() { Name = "Living Seating", Material = "MR Plywood", Description = "Paint: DUCO", Rate = 1600 },
            new() { Name = "Pooja Mandir", Material = "MR Plywood", Description = "CNC Cutting and Design | Paint: DUCO", Rate = 1900 },
            new() { Name = "Crockery Unit", Material = "MR Plywood", Description = "", Rate = 1200 },
            new() { Name = "Crockery Unit: Glass Door", Material = "", Description = "Saint Gobain Glasses", Rate = 4000 },
            new() { Name = "Wall Cover Unit", Material = "Gypsum Cover", Description = "", Rate = 500 },
            new() { Name = "Room 1: Wardrobe", Material = "MR Plywood", Description = "", Rate = 1300 },
            new() { Name = "Room 1: Loft", Material = "MR Plywood", Description = "", Rate = 900 },
            new() { Name = "Room 1: Curved Table", Material = "MR Plywood", Description = "MS Rod Support", Rate = 1600 },
            new() { Name = "Room 1: False Ceiling", Material = "Gypsum", Description = "", Rate = 70 },
            new() { Name = "Vanity Unit", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Bathroom Storage", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Bathroom Floor Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "Bathroom Wall Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "Bathroom Upper Cover", Material = "Knauf Channel", Description = "PVC Sheets", Rate = 500 },
            new() { Name = "Room 1 Bathroom Mirror", Material = "", Description = "", Rate = 4000 },
            new() { Name = "MBR Wardrobe", Material = "MR Plywood", Description = "", Rate = 1300 },
            new() { Name = "MBR Loft", Material = "MR Plywood", Description = "", Rate = 900 },
            new() { Name = "MBR Dressing Unit", Material = "MR Plywood", Description = "Saint Gobain Mirror with AL Frame", Rate = 1600 },
            new() { Name = "MBR Panel", Material = "", Description = "Gyproc Panel", Rate = 300 },
            new() { Name = "MBR False Ceiling", Material = "", Description = "Gyproc", Rate = 70 },
            new() { Name = "MBR Vanity", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "MBR Storage", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "MBR Floor Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "MBR Wall Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "MBR Upper Bathroom Covering", Material = "Knauf Channel", Description = "PVC Ceiling", Rate = 500 },
            new() { Name = "MBR Bed Back", Material = "", Description = "", Rate = 400 },
            new() { Name = "MBR Bathroom Mirror", Material = "", Description = "", Rate = 4000 },
            new() { Name = "MBR Bathroom Glass Separator", Material = "", Description = "", Rate = 900 },
            new() { Name = "Common Bathroom Floor Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "Common Bathroom Wall Tiles", Material = "", Description = "", Rate = 500 },
            new() { Name = "Common Bathroom Upper Covering", Material = "", Description = "", Rate = 500 },
            new() { Name = "Common Bathroom Vanity", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Common Bathroom Storage", Material = "BWP Plywood", Description = "", Rate = 1600 },
            new() { Name = "Common Bathroom Mirror", Material = "", Description = "", Rate = 4000 },
            new() { Name = "TV Unit", Material = "", Description = "", Rate = 800 },
            new() { Name = "Living Room False Ceiling", Material = "", Description = "Gyproc", Rate = 70 },
            new() { Name = "Design Cost", Material = "", Description = "3D Drawing | 2D Drawing - Complete Internal Layout | Working Drawing", Rate = 50000 }
        };
    }
}
