using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InyarthaApp.Models;
using InyarthaApp.Services;

namespace InyarthaApp.Controllers;

[Authorize]
public class QuoteController : Controller
{
    private readonly IQuoteService _qs;
    private readonly PdfService _pdf;

    public QuoteController(IQuoteService qs, PdfService pdf)
    {
        _qs = qs;
        _pdf = pdf;
    }

    public async Task<IActionResult> Index()
        => View(await _qs.GetAllAsync());

    [HttpGet]
    public IActionResult Create() => View(new Quote());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Quote model)
    {
        ModelState.Remove(nameof(model.Budget));
        ModelState.Remove(nameof(model.Timeline));
        ModelState.Remove(nameof(model.Notes));
        if (!ModelState.IsValid) return View(model);
        model.Id = Guid.NewGuid().ToString("N")[..8].ToUpper();
        model.CreatedAt = DateTime.Now;
        model.CreatedBy = User.Identity?.Name ?? "admin";
        try
        {
            await _qs.SaveAsync(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to save: {ex.Message}");
            return View(model);
        }
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
        ModelState.Remove(nameof(model.Budget));
        ModelState.Remove(nameof(model.Timeline));
        ModelState.Remove(nameof(model.Notes));
        if (!ModelState.IsValid) return View(model);
        try
        {
            await _qs.SaveAsync(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Failed to save: {ex.Message}");
            return View(model);
        }
        TempData["Success"] = $"Quote #{model.Id} updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var q = await _qs.GetByIdAsync(id);
        if (q == null) return NotFound();
        return View(q);
    }

    [HttpGet]
    public async Task<IActionResult> Preview(string id)
    {
        if (string.IsNullOrEmpty(id) || !_pdf.Exists(id))
            return NotFound();
        var path = _pdf.GetPdfPath(id);
        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        return File(bytes, "application/pdf");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeneratePdfs(string quoteIds)
    {
        if (string.IsNullOrEmpty(quoteIds))
        {
            TempData["Success"] = "No quotes selected.";
            return RedirectToAction(nameof(Index));
        }
        var ids = quoteIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var count = await _qs.GeneratePdfsAsync(ids);
        TempData["Success"] = $"{count} PDF(s) generated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        await _qs.DeleteAsync(id);
        TempData["Success"] = "Quote deleted.";
        return RedirectToAction(nameof(Index));
    }
}
