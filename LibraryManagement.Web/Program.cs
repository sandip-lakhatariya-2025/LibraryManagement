using System.Reflection;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AutoMapper;
using LibraryManagement.Common;
using LibraryManagement.Common.MappingProfiles;
using LibraryManagement.DataAccess.Data;
using LibraryManagement.Services;
using LibraryManagement.Utility.MappingProfiles;
using LibraryManagement.Web;
using LibraryManagement.Web.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;

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

    builder.Services.RegisterServices();
    builder.Services.RegisterSecurityServices(builder.Configuration);
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddAuthorization();
    
    builder.Services.AddAutoMapper(typeof(MapperConfig));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });

    builder.Services.AddApiVersioning(options => 
    {
        options.DefaultApiVersion = new ApiVersion(2);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options => 
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);

        options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

        options.OperationFilter<AddCommonHeadersOperationFilter>();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme{
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme 
                {
                    Reference = new OpenApiReference 
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

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