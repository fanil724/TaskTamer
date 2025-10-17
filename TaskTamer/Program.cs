using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;
using System.Security.Claims;
using System.Text;
using TaskTamer_Application.Service;
using TaskTamer_Logic.Stores;
using TaskTamer_Persistence.DataAccess;
using TaskTamer_Persistence.Repository;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerSentEvents(options =>
{
    options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
    options.KeepaliveInterval = 30; 
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

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


builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

AddServices(builder);

var app = builder.Build();
app.MapServerSentEvents("/notifications-stream");

app.UseCors("AllowReactApp");


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Task Tamer"));
}

app.UseExceptionHandler();
app.MapControllers();

app.Run();


void AddServices(WebApplicationBuilder builder)
{
    builder.Services.AddScoped<UserService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<RequestService>();
    builder.Services.AddScoped<IRequestRepository, RequestRepository>();
    builder.Services.AddScoped<RequestHistoryService>();
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

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"] ?? "")),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies[builder.Configuration["Jwt:token"] ?? "token"];
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
        options.AddPolicy("MinimumAccessLevel", policy =>
            policy.RequireClaim("AccessLevel", "2", "3", "4", "5"));
    });
}