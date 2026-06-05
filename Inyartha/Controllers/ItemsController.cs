using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InyarthaApp.Models;
using InyarthaApp.Services;

namespace InyarthaApp.Controllers;

[Authorize]
public class ItemsController : Controller
{
    private readonly ItemsService _items;

    public ItemsController(ItemsService items) => _items = items;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _items.GetAllAsync();
        return Json(list);
    }

    public async Task<IActionResult> Index()
    {
        var list = await _items.GetAllAsync();
        return View(list);
    }

    [HttpGet]
    public IActionResult Create() => View(new ItemTemplate());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ItemTemplate model)
    {
        ModelState.Remove(nameof(model.Description));
        ModelState.Remove(nameof(model.PricingUnit));
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Item name is required.");
            return View(model);
        }
        if (model.Rate < 0)
        {
            ModelState.AddModelError("Rate", "Rate must be zero or positive.");
            return View(model);
        }
        try
        {
            await _items.AddAsync(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to save: {ex.Message}");
            return View(model);
        }
        TempData["Success"] = $"Item \"{model.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string name, string material)
    {
        var item = await _items.GetByNameMaterialAsync(name, material);
        if (item == null) return NotFound();
        ViewBag.OriginalName = name;
        ViewBag.OriginalMaterial = material;
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ItemTemplate model, string originalName, string originalMaterial)
    {
        ModelState.Remove(nameof(model.Description));
        ModelState.Remove(nameof(model.PricingUnit));
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("Name", "Item name is required.");
            ViewBag.OriginalName = originalName;
            ViewBag.OriginalMaterial = originalMaterial;
            return View(model);
        }
        if (model.Rate < 0)
        {
            ModelState.AddModelError("Rate", "Rate must be zero or positive.");
            ViewBag.OriginalName = originalName;
            ViewBag.OriginalMaterial = originalMaterial;
            return View(model);
        }
        try
        {
            await _items.UpdateAsync(originalName, originalMaterial, model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to update: {ex.Message}");
            ViewBag.OriginalName = originalName;
            ViewBag.OriginalMaterial = originalMaterial;
            return View(model);
        }
        TempData["Success"] = $"Item \"{model.Name}\" updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string name, string material)
    {
        await _items.DeleteAsync(name, material);
        TempData["Success"] = $"Item \"{name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Copy(string name, string material)
    {
        await _items.CopyAsync(name, material);
        TempData["Success"] = $"Item \"{name}\" copied.";
        return RedirectToAction(nameof(Index));
    }
}
