using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SPTS_Repository.Entities;
using SPTS_Repository.Interface.Admin;
using SPTS_Repository.Interface.Auth;
using SPTS_Repository.Interface.Giangvien;
using SPTS_Repository.Interface.Shared;
using SPTS_Repository.Interface.Sinhvien;
using SPTS_Repository.Repositories.Admin;
using SPTS_Repository.Repositories.Auth;
using SPTS_Repository.Repositories.Giangvien;
using SPTS_Repository.Repositories.Quantrivien;
using SPTS_Repository.Repositories.Shared;
using SPTS_Repository.Repositories.Sinhvien;
using SPTS_Service.Interface.Admin;
using SPTS_Service.Interface.Auth;
using SPTS_Service.Interface.Domain;
using SPTS_Service.Interface.Giangvien;
using SPTS_Service.Interface.Student;
using SPTS_Service.Services.Auth;
using SPTS_Service.Services.Domain;
using SPTS_Service.Services.Giangvien;
using SPTS_Service.Services.Quantrivien;
using SPTS_Service.Services.Sinhvien;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DI for DB Context
builder.Services.AddDbContext<SptsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectedDb"))
);

//Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "COMPASS";
        opt.LoginPath = "/Home/Login";
        opt.AccessDeniedPath = "/Home/Login";

        // opt.ExpireTimeSpan = TimeSpan.FromDays(7);

        opt.SlidingExpiration = true; // Cookie tự gia hạn khi user active
        opt.Cookie.SameSite = SameSiteMode.Lax;
        opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opt.Cookie.HttpOnly = true;
    });
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// repository for student
builder.Services.AddScoped<IAlertStudentRepository, AlertStudentRepository>();
builder.Services.AddScoped<ICourseStudentRepository, CourseStudentRepository>();
builder.Services.AddScoped<IGPAStudentRepository, GPAStudentRepository>();
builder.Services.AddScoped<INotificationStudentRepository, NotificationStudentRepository>();
builder.Services.AddScoped<IProfileStudentRepository, ProfileStudentRepository>();
builder.Services.AddScoped<ITermStudentRepository, TermStudentRepository>();
// service for student
builder.Services.AddScoped<IDashboardStudentService, DashboardStudentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// repository for teacher
builder.Services.AddScoped<IAlertTeacherRepository, AlertTeacherRepository>();
builder.Services.AddScoped<IChartTeacherRepository, ChartTeacherRepository>();
builder.Services.AddScoped<IDashboardTeacherRepository, DashboardTeacherRepository>();
builder.Services.AddScoped<IGradeTeacherRepository, GradeTeacherRepository>();  
builder.Services.AddScoped<INotificationTeacherRepository, NotificationTeacherRepository>();
builder.Services.AddScoped<IProfileTeacherRepository, ProfileTeacherRepository>();
builder.Services.AddScoped<ISectionTeacherRepository, SectionTeacherRepository>();
builder.Services.AddScoped<ITermTeacherRepository, TermTeacherRepository>();
// Service for teacher 
builder.Services.AddScoped<IDashboardTeacherService, DashboardTeacherService>();
builder.Services.AddScoped<IGradeTeacherService, GradeTeacherService>();
builder.Services.AddScoped<INotificationTeacherService, NotificationTeacherService>();
builder.Services.AddScoped<IProfileTeacherService, ProfileTeacherService>();
builder.Services.AddScoped<ISectionTeacherService, SectionTeacherService>();

// Repository for admin
builder.Services.AddScoped<IKPIRepository, KPIRepositorry>();
builder.Services.AddScoped<ISectionManagementRepository, SectionManagementRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<IStatisticsRepository, StatisticsRepository>();
builder.Services.AddScoped<ITermRepository, TermRepository>();
builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
// Service for admin
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();


builder.Services.AddScoped<IGpaCalculationService, GpaCalculationService>();
builder.Services.AddScoped<IAlertSyncService, AlertSyncService>();
builder.Services.AddScoped<INotificationDomainService, NotificationDomainService>();
builder.Services.AddScoped<ITermGpaRepository, TermGpaRepository>();

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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
