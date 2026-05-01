using Microsoft.AspNetCore.Mvc;

namespace InyarthaApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new InyarthaApp.Models.ErrorViewModel
        {
            RequestId = System.Diagnostics.Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
        });
}