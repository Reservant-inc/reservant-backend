using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p =>
    {
        p.SetIsOriginAllowed(_ => true);
        p.AllowAnyHeader();
        p.AllowCredentials();
        p.AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddScoped<DbSeeder>();

builder.Services.AddAuthorization();
builder.Services
    .AddIdentity<User, IdentityRole>(o =>
    {
        o.SignIn.RequireConfirmedEmail = false;
        o.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    o.Events.OnRedirectToAccessDenied = ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var filePath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
    options.IncludeXmlComments(filePath, includeControllerXmlComments: true);
});

builder.Services.AddControllers();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RestaurantService>();
builder.Services.AddScoped<RestaurantGroupService>();
builder.Services.AddScoped<RestaurantGroupService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

    if (await context.Database.EnsureCreatedAsync())
    {
        await seeder.SeedDataAsync();
    }
}

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
