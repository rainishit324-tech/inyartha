using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InyarthaApp.Models;

namespace InyarthaApp.Controllers;

public class AccountController : Controller
{
    private static readonly Dictionary<string, string> _users =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "admin",  "Inyartha@2025" },
            { "vikash", "Vikash@123"    },
        };

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        if (!_users.TryGetValue(model.Username, out var pwd) || pwd != model.Password)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Admin"),
        };
        var identity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/Quote" : returnUrl);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}