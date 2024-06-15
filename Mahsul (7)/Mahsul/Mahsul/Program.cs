using Mahsul.Data;
using Mahsul.Helpers;
using Mahsul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

// SMTP istemcisini olu�tur
var smtpClient = new SmtpClient(smtpSettings.Server)
{
    Port = smtpSettings.Port,
    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
    EnableSsl = true,
};
builder.Services.AddSingleton(smtpClient);
builder.Services.AddSingleton(smtpSettings);
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Register the ProductMaintenanceService as a hosted service
builder.Services.AddHostedService<ProductMaintenanceService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=FullIndex}/{id?}");
app.MapRazorPages();

app.Run();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "purchases",
        pattern: "purchases",
        defaults: new { controller = "Purchase", action = "Index" });

    endpoints.MapControllerRoute(
        name: "statistics",
        pattern: "{controller=Statistics}/{action=Index}/{id?}");
});
