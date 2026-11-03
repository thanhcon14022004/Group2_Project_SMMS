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

// Hỗ trợ cấu hình riêng cho từng máy developer (không bị đẩy lên git)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// 1. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connStr = builder.Configuration.GetConnectionString("MyCNN");
    options.UseSqlServer(connStr);
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
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    // Chấp nhận cả trường hợp người dùng dán trực tiếp token không có chữ Bearer
                    context.Token = authHeader.Trim();
                }
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("JWT Auth Failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            // Ngăn chặn trả về response rỗng hoặc redirect
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var responseObj = new
            {
                success = false,
                statusCode = 401,
                message = "Yêu cầu xác thực. Vui lòng cung cấp JWT Bearer Token hợp lệ.",
                error = "Unauthorized"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(responseObj);
            await context.Response.WriteAsync(json);
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var responseObj = new
            {
                success = false,
                statusCode = 403,
                message = "Bạn không có quyền truy cập vào tài nguyên này.",
                error = "Forbidden"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(responseObj);
            await context.Response.WriteAsync(json);
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
        Description = "Nhập trực tiếp chuỗi JWT Token (Swagger sẽ tự động gắn tiền tố Bearer)",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
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
