using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartCard.Data;
using SmartCard.Extensions;
using SmartCard.Hubs;
using SmartCard.Middleware;
using SmartCard.Models;
using SmartCard.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// 👇 Add this line to enable static web assets
builder.WebHost.UseStaticWebAssets();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers() // Add this for API controllers
    .AddJsonOptions(options =>
    {
        // Configurer la sérialisation/désérialisation JSON pour les enums
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseNpgsql(
//        builder.Configuration.GetConnectionString("DefaultConnection")
//    ));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme; // Pour l'authentification web
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SmartCardAPI",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SmartCardUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForSmartCardApp2024!"))
    };
});

// Configure les cookies d'authentification pour Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";

    // Configuration pour que les cookies soient inclus avec les appels API
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Adapter selon votre environnement
    options.Cookie.SameSite = SameSiteMode.Lax; // Use Lax for better cross-site compatibility
    options.Cookie.Name = "SmartCardAuth"; // Explicit cookie name
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set expiration time
    options.SlidingExpiration = true; // Refresh the cookie on each request within the expiration period

    // For API controllers, we want to return 401/403 for unauthorized requests
    // while maintaining normal redirect behavior for page requests
    options.Events.OnRedirectToLogin = context =>
    {
        // Return 401 for API requests, otherwise use default redirect behavior
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        // For other requests, use default behavior by not handling the event
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        // Return 403 for API requests, otherwise use default redirect behavior
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        // For other requests, use default behavior by not handling the event
        return Task.CompletedTask;
    };
});

// Configuration des options d'Identity
builder.Services.PostConfigure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
});


// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add custom services
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IFuelQuotaService, FuelQuotaService>();
builder.Services.AddScoped<IConsumptionService, ConsumptionService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();

// Ensure all required services are registered
builder.Services.AddHttpContextAccessor();

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fuel Distribution API", Version = "v1" });

    // Specify the path for XML comments if any exist
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT Authentication support to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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


// CORS policy to allow requests from local network (for testing purposes)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalNetwork", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(origin => true);
    });
});

// Dans Program.cs ou Startup.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactNative", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});




var app = builder.Build();

app.UseCors("AllowLocalNetwork");

// Configure the HTTP request pipeline.
// Use custom error handling middleware first
app.UseErrorHandling();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseCors("AllowReactNative");

// Initialize the database
await app.UseDatabaseInitializer();

// Log all exceptions during development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Configure Swagger for development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartCard API v1");
    c.RoutePrefix = "swagger";
});


//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Use CORS
app.UseCors("AllowAll");

// Map API controllers explicitly
app.MapControllers();

app.MapStaticAssets();

// Rediriger la racine vers la page de connexion
app.MapGet("/", () => Results.Redirect("/Login"));

app.MapRazorPages()
   .WithStaticAssets();

// Map SignalR hubs
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
