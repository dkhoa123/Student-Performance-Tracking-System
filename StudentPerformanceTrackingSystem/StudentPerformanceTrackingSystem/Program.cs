using Microsoft.EntityFrameworkCore;
using SPTS_Repository;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface;
using SPTS_Service;
using SPTS_Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DI for DB Context
builder.Services.AddDbContext<SptsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"))
);

builder.Services.AddScoped<ISinhVienRepository, SinhVienRepository>();
builder.Services.AddScoped<ISinhVienService, SinhVienService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
