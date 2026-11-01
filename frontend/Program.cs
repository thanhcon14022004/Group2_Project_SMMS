using project_PRN232.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add MVC with Views
builder.Services.AddControllersWithViews();

// Add HttpContextAccessor and Session for managing JWT & user state
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure typed HttpClient to communicate with Backend Web API
builder.Services.AddHttpClient<ApiClientService>(client =>
{
    var backendUrl = builder.Configuration["BackendApiUrl"] ?? "http://127.0.0.1:5050/api/";
    client.BaseAddress = new Uri(backendUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

