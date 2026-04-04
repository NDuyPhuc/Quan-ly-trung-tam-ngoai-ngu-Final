using Quan_ly_trung_tam_ngoai_ngu.Services.Interfaces;
using Quan_ly_trung_tam_ngoai_ngu.Services.Mocks;
using Quan_ly_trung_tam_ngoai_ngu.Services;
using Quan_ly_trung_tam_ngoai_ngu.Services.Sql;
using Quan_ly_trung_tam_ngoai_ngu.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SmtpMailOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddSingleton<MockDataService>();
builder.Services.AddScoped<IMockDataService, SqlServerDataService>();
builder.Services.AddScoped<IDemoAuthService, SqlAuthService>();
builder.Services.AddScoped<ILanguageCenterManagementService, SqlLanguageCenterManagementService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddSingleton<IPublicSiteContentService, PublicSiteContentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
