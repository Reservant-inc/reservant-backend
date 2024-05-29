using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Reservant.Api;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Options;
using Reservant.Api.Services;
using Reservant.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.ConfigSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<FileUploadsOptions>()
    .BindConfiguration(FileUploadsOptions.ConfigSection)
    .ValidateDataAnnotations()
    .Validate(
        o => Path.EndsInDirectorySeparator(o.GetFullSavePath()),
        $"{nameof(FileUploadsOptions.SavePath)} must end with /")
    .Validate(
        o => !Path.EndsInDirectorySeparator(o.ServePath),
        $"{nameof(FileUploadsOptions.ServePath)} must not end with /")
    .ValidateOnStart();

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

builder.Services.AddDbContext<ApiDbContext>();

builder.Services.AddScoped<DbSeeder>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var jwtOptions = builder.Configuration.GetSection(JwtOptions.ConfigSection)
            .Get<JwtOptions>() ?? throw new InvalidOperationException("Failed to read JwtOptions");

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtOptions.GetKeyBytes()),
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();
builder.Services
    .AddIdentityCore<User>(o =>
    {
        o.SignIn.RequireConfirmedEmail = false;
        o.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var assembly = Assembly.GetExecutingAssembly();
    var buildTime = assembly.GetCustomAttribute<BuildTimeAttribute>()?.BuildTime;

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reservant API",
        Version = "v1",
        Description = $"""
                       Sekude

                       *Built at {buildTime:HH:mm:ss, d MMM yyyy}*
                       """
    });

    var filePath = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
    options.IncludeXmlComments(filePath, includeControllerXmlComments: true);

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Description = "Your JWT Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "JWT Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, new List<string>() }
    });
});

builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<ValidationService>();

var services = Assembly.GetExecutingAssembly()
    .GetTypes().Where(t => t.Name.EndsWith("Service"));
foreach (var service in services)
{
    builder.Services.AddScoped(service);
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var debugService = scope.ServiceProvider.GetRequiredService<DebugService>();
    var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();

    var fileUploadsOptions = scope.ServiceProvider.GetRequiredService<IOptions<FileUploadsOptions>>().Value;
    if (!Path.Exists(fileUploadsOptions.GetFullSavePath()))
    {
        Directory.CreateDirectory(fileUploadsOptions.GetFullSavePath());
    }

    if (app.Environment.IsProduction())
    {
        await debugService.RecreateDatabase();
    }
    else if (await context.Database.EnsureCreatedAsync())
    {
        await seeder.SeedDataAsync();
    }
}

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var fileUploadsOptions = scope.ServiceProvider.GetRequiredService<IOptions<FileUploadsOptions>>().Value;
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fileUploadsOptions.GetFullSavePath()),
        RequestPath = fileUploadsOptions.ServePath
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
