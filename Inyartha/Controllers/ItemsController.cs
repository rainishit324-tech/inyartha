using InyarthaApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace InyarthaApp.Controllers;

public class ItemsController : Controller
{
    private readonly ItemsService _items;

    public ItemsController(ItemsService items) => _items = items;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _items.GetAllAsync();
        return Json(list);
    }
}
