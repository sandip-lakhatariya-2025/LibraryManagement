using System.Reflection;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.DataAccess.IRepository;
using LibraryManagement.DataAccess.Repository;
using LibraryManagement.Services;
using LibraryManagement.Services.IServices;
using LibraryManagement.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DatabaseAddress");
builder.Services.AddDbContext<ApplicationDbContext>(q => {
    q.UseNpgsql(conn, b => b.MigrationsAssembly("LibraryManagement.Web"));
    q.LogTo(Console.WriteLine, LogLevel.Information);  // <-- Enable SQL logging here
    q.EnableSensitiveDataLogging();                    // <-- Optional: show parameter values
});

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddControllers(options => {
    options.Filters.Add<ApiLoggingFilterAttribute>();
});

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

    options.OperationFilter<ApiKeyFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.Run();