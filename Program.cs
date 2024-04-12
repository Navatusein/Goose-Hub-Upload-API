using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using MassTransit;
using UploadApi.Services;
using UploadApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Logger
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateBootstrapLogger();

// Add logger
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Upload API",
        Description = "Upload API",
        TermsOfService = new Uri("https://github.com/openapitools/openapi-generator"),
        Contact = new OpenApiContact
        {
            Name = "Bohdan",
            Url = new Uri("https://github.com/Navatusein"),
            Email = "boghdan.kutsulima@gmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "Mit License",
            Url = new Uri("https://github.com/Navatusein/Goose-Hub-Upload-API/blob/main/LICENSE")
        },
        Version = "v1",
    });

    options.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()!.GetName().Name}.xml");

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.OperationFilter<SecurityRequirementsOperationFilter>(true, jwtSecurityScheme.Reference.Id);
});

// Configure Frontend Authentication Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AuthorizeJWT:Issuer"],
            ValidAudience = builder.Configuration["AuthorizeJWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthorizeJWT:Key"]))
        };
    });

// Add Minio Service
builder.Services.AddSingleton<MinioService>();

// Add MassTransit
builder.Services.AddMassTransit(options =>
{
    options.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("upload-api", false));

    options.UsingRabbitMq((context, config) =>
    {
        var host = builder.Configuration.GetSection("RabbitMq:Host").Get<string>();
        var virtualHost = builder.Configuration.GetSection("RabbitMq:VirtualHost").Get<string>();

        config.Host(host, virtualHost, host =>
        {
            host.Username(builder.Configuration.GetSection("RabbitMq:Username").Get<string>());
            host.Password(builder.Configuration.GetSection("RabbitMq:Password").Get<string>());
        });

        config.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(option =>
    {
        option.DocumentTitle = "Upload API";
    });
}

// Add Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors(options => {
    string[] origins = builder.Configuration.GetSection("Origins").Get<string[]>()!;

    options.WithOrigins(origins);
    options.AllowAnyMethod();
    options.AllowAnyHeader();
    options.AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

