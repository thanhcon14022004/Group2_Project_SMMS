using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using project_PRN232.Data;
using project_PRN232.DTOs;
using project_PRN232.Models.Entities;
using project_PRN232.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Mycnn"));
});

// 2. Authentication & JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SMMS_Super_Secret_Key_For_Jwt_Authentication_PRN232_Course_2026!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SMMS_Server";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SMMS_Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = false, // Linh hoạt trong dev
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT Auth Failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT Challenge: Error={context.Error}, Desc={context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("LecturerOnly", policy => policy.RequireRole("Lecturer"));
    options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
    options.AddPolicy("AdminOrLecturer", policy => policy.RequireRole("Admin", "Lecturer"));
});

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Register Application Services
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurriculumService, CurriculumService>();
builder.Services.AddScoped<IStudentAllocationService, StudentAllocationService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IClassTransferService, ClassTransferService>();
builder.Services.AddScoped<ISemesterClosingService, SemesterClosingService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBackupRestoreService, BackupRestoreService>();

// 5. Controllers & JSON Options & OData
builder.Services.AddControllers()
    .AddOData(options =>
    {
        options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
               .AddRouteComponents("odata", GetEdmModel());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<SubjectDto>("Subjects");
    builder.EntitySet<MajorDto>("Majors");
    builder.EntitySet<SpecializationDto>("Specializations");
    builder.EntitySet<AdministrativeClassDto>("AdministrativeClasses");
    builder.EntitySet<CourseSectionDto>("CourseSections");
    builder.EntitySet<SemesterDto>("Semesters");
    return builder.GetEdmModel();
}

// 6. Swagger with JWT Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SMMS API - Student Mark Management System",
        Version = "v1",
        Description = "API cho Hệ thống quản lý điểm sinh viên (PRN232 Project dựa trên report.docx)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Ví dụ: 'Bearer {token}'",
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 7. Auto Migrate & Seed Database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        DbInitializer.Initialize(db);
        await project_PRN232.Data.DatabaseFontFixer.FixMojibakeAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khởi tạo hoặc migrate database");
    }
}

// 8. Pipeline configuration
app.UseRouting();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMMS API v1");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirect root to Swagger API documentation
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
