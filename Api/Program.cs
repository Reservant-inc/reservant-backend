using System.Reflection;
using System.Text.Json.Serialization;
using FluentValidation;
using Reservant.LogsViewer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Reservant.Api.Data;
using Reservant.Api.Documentation;
using Reservant.Api.Identity;
using Reservant.Api.Options;
using Reservant.Api.Push;
using Reservant.Api.Services;
using Reservant.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConfigurationOptions();

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

var logDbConnectionString = builder.Configuration.GetConnectionString("LogDb")
    ?? throw new InvalidOperationException("Connection string 'LogDb' not found");
builder.Services.AddLogsViewer(o => o.UseSqlite(logDbConnectionString));

builder.Services.AddSwaggerServices();
builder.Services.AddSingleton<ProblemDetailsFactory, CustomProblemDetailsFactory>();
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddScoped<GeometryFactory>(_ =>
    NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddScoped<ValidationService>();

builder.Services.AddDbContext<ApiDbContext>();
builder.Services.AddScoped<DbSeeder>();
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddPushServices();
builder.Services.AddBusinessServices();

var app = builder.Build();

await app.EnsureDatabaseCreatedAndSeeded();

app.Services.RegisterLogsViewerProvider();

app.UseCors();

app.UseLogsViewerUI();

app.UseSwagger();
app.UseSwaggerUI();

app.UseWebSockets();
app.UsePushMiddleware();

app.UseHttpLogging();

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
