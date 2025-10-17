using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using TaskTamer_Admin.Data;
using TaskTamer_Application.Service;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;
using TaskTamer_Persistence.Repository;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (Environment.MachineName.Contains("NFS"))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContextWork"));
    }
    else if(Environment.MachineName.Contains("DESKTOP"))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(AppDbContext)));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContextCont"));
    }
});


builder.Services.AddDbContext<AdminContext>(options =>
{
    if (Environment.MachineName.Contains("NFS"))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("AdminContextWork"));
    }
    else if(Environment.MachineName.Contains("DESKTOP"))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(AdminContext)));
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbContextCont"));
    }
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<AdminContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.LogoutPath = "/Identity/Account/Logout";
    
});

AddServices(builder);

builder.Services.AddControllersWithViews();

var app = builder.Build();


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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "area",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();


void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<RequestService>();
    builder.Services.AddScoped<IRequestRepository, RequestRepository>();
    builder.Services.AddScoped<IRequestHistoryRepository, RequestHistoryRepository>();
    builder.Logging.ClearProviders();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IAuthlogRepository, AuthlogRepository>();
    builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();
    builder.Services.AddRazorPages();
    builder.Services.AddScoped<EmailSender>();

    builder.Services.AddScoped<DepartmentService>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<RoleService>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<RequestStatusService>();
    builder.Services.AddScoped<IRequestStatusRepository, RequestStatusRepository>();
    builder.Services.AddScoped<RequestTypeService>();
    builder.Services.AddScoped<IRequestTypeRepository, RequestTypeRepository>();
    builder.Services.AddScoped<PositionService>();
    builder.Services.AddScoped<IPositionRepository, PositionRepository>();
    builder.Services.AddScoped<EmployeeService>();
    builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
    builder.Services.AddScoped<EquipmentService>();
    builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
}