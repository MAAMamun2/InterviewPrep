using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using InterviewPrep.Api.Middleware;
using InterviewPrep.Application.Common.Behaviors;
using InterviewPrep.Infrastructure;
using InterviewPrep.Infrastructure.Auth;
using InterviewPrep.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---- MediatR: scans Application assembly for every IRequestHandler/INotificationHandler ----
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(InterviewPrep.Application.Members.Commands.CreateMember.CreateMemberCommand).Assembly);

    // Pipeline order = execution order: every request is Logged, then Validated, then Timed,
    // THEN the actual handler runs. Same mental model as ASP.NET middleware ordering.
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(InterviewPrep.Application.Members.Commands.CreateMember.CreateMemberCommand).Assembly);

// ---- Infrastructure (EF Core, repositories, JWT, caching, background job) ----
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "InterviewPrep API", Version = "v1" });

    // Lets you click "Authorize" in the Swagger UI and paste a JWT once for every endpoint --
    // minor DX detail, but shows you know your way around Swashbuckle configuration.
    c.AddSecurityDefinition("Bearer", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ---- JWT authentication ----
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---- CORS: only the two frontend dev servers, with credentials for the Authorization header ----
const string CorsPolicy = "FrontendDevServers";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins("http://localhost:4200", "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// ---- Rate limiting: brute-force defense on the login endpoint specifically ----
builder.Services.AddRateLimiter(options =>
{
    // GOTCHA worth knowing cold: the built-in limiter's RejectionStatusCode defaults to 503
    // (Service Unavailable), NOT 429 (Too Many Requests) -- you must set this explicitly to get
    // the semantically correct response code. Caught this myself by actually testing the 6th
    // request in a 1-minute window rather than assuming the default was right.
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

// ---- Seed the database on startup so the login pages work with zero manual setup ----
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InterviewPrepDbContext>();
    await DbInitializer.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Pipeline order matters: exception handling outermost, then timing, then the built-in
// authentication/authorization/CORS/rate-limiting middleware, then routing to controllers.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
