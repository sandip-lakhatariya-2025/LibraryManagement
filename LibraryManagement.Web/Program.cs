using System.Reflection;
using LibraryManagement.Common;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.DataAccess.Repository;
using LibraryManagement.DataAccess.Repository.IRepository;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var logFilePath = Path.Combine(exeFolder ?? ".", "Logs", $"API_Logs_{DateTime.Now.ToString("MM_dd_yyyy")}.log");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Error)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        logFilePath,
        rollingInterval: RollingInterval.Infinite,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("starting server.");
    var builder = WebApplication.CreateBuilder(args);

    var conn = builder.Configuration.GetConnectionString("DatabaseAddress");
    builder.Services.AddDbContext<ApplicationDbContext>(q => {
        q.UseNpgsql(conn, b => b.MigrationsAssembly("LibraryManagement.Web"));
    });

    builder.Host.UseSerilog();

    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options => 
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Library Management",
            Version = "v1"
        });

        options.EnableAnnotations();
        options.OperationFilter<AddCommonHeadersOperationFilter>();
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseMiddleware<ApiKeyMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    app.UseMiddleware<ApiLoggingMiddleware>();

    app.MapControllers();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "server terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}