using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<InyarthaApp.Services.ExcelService>();
builder.Services.AddSingleton<InyarthaApp.Services.PdfService>();
builder.Services.AddSingleton<InyarthaApp.Services.ItemsService>();
builder.Services.AddSingleton<InyarthaApp.Services.IQuoteService,
    InyarthaApp.Services.QuoteService>();

var app = builder.Build();

/* ── Seed items.xlsx on first run ── */
using (var scope = app.Services.CreateScope())
{
    var itemsSvc = scope.ServiceProvider.GetRequiredService<InyarthaApp.Services.ItemsService>();
    await itemsSvc.SeedIfNotExistsAsync();
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();