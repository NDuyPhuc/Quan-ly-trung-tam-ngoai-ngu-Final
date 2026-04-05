using Quan_ly_trung_tam_ngoai_ngu.Models;
using Quan_ly_trung_tam_ngoai_ngu.Services;
using Quan_ly_trung_tam_ngoai_ngu.Services.Ef;
using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.Services.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Quan_ly_trung_tam_ngoai_ngu.Data;
using Quan_ly_trung_tam_ngoai_ngu.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("LanguageCenterDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(3);
            sqlOptions.CommandTimeout(15);
        });
});
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SmtpMailOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IAccountPasswordService, AccountPasswordService>();
builder.Services.AddScoped<ILanguageCenterReadService, EfLanguageCenterReadService>();
builder.Services.AddScoped<IAccountAuthService, EfAuthService>();
builder.Services.AddScoped<ILanguageCenterManagementService, EfLanguageCenterManagementService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddSingleton<IPublicSiteContentService, PublicSiteContentService>();

var app = builder.Build();

await app.Services.ApplySecurityBackfillAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
