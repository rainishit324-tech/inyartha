using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InyarthaApp.Models;
using InyarthaApp.Services;

namespace InyarthaApp.Controllers;

[Authorize]
public class QuoteController : Controller
{
    private readonly IQuoteService _qs;
    public QuoteController(IQuoteService qs) => _qs = qs;

    public async Task<IActionResult> Index()
        => View(await _qs.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new Quote());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Quote model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Id = Guid.NewGuid().ToString("N")[..8].ToUpper();
        model.CreatedAt = DateTime.Now;
        model.CreatedBy = User.Identity?.Name ?? "admin";
        await _qs.SaveAsync(model);
        TempData["Success"] = $"Quote #{model.Id} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var q = await _qs.GetByIdAsync(id);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Quote model)
    {
        if (!ModelState.IsValid) return View(model);
        await _qs.SaveAsync(model);
        TempData["Success"] = $"Quote #{model.Id} updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var q = await _qs.GetByIdAsync(id);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        await _qs.DeleteAsync(id);
        TempData["Success"] = "Quote deleted.";
        return RedirectToAction(nameof(Index));
    }
}