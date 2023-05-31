using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<BlogUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<BlogDbContext>()
    .AddDefaultUI();

//builder.Services.AddAuthentication().AddFacebook(option =>
//{
//    option.AppId = "555515893195524";
//    option.AppSecret = "39737841968438e66bd0e9038f784e1b";
//});

//builder.Services.AddAuthentication().AddGoogle(option =>
//{
//    option.ClientId = "386404887961-ujomiu71hdijd9qpv9l2bu4m1u3u4dbb.apps.googleusercontent.com";
//    option.ClientSecret = "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";
//});

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlogPosts}/{action=Index}/{id?}");

app.Run();
