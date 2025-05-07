using Scalar.AspNetCore;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using WebAppForMiniProfiler.Abstract;
using WebAppForMiniProfiler.Context;
using WebAppForMiniProfiler.Helpers;
using WebAppForMiniProfiler.Service;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

builder.Services.AddScoped<DapperContext>();

builder.Services.AddScoped<ITestService, TestService>();

builder.Services.AddMemoryCache();

var memoryStorage = new CustomMiniProfilerMemoryStorage(TimeSpan.FromHours(24), maxEntries: 1000);
builder.Services.AddSingleton<IAsyncStorage>(memoryStorage);
builder.Services.AddMiniProfiler(options =>
{
    options.RouteBasePath = "/mini-profiler";// MiniProfiler UI endpoint'lerinin base route'u
    options.ColorScheme = ColorScheme.Dark;//Tema rengini belirler.
    options.ShouldProfile = request =>
    {
        var path = request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.Contains("/docs/") || path.Contains("/docs"))
        {
            return false;// /docs/ ve /docs isteklerini profile ekleme diyorum.
        }
        return true;
    };
    options.Storage = memoryStorage;
    options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();// SQL sorgularini detayli sekilde goster
    options.TrackConnectionOpenClose = true;// DB baglantisi acik-kapama gibi detaylari da dahil et
    // "Trivial" (cok kisa suren adimlar) da popup ve results'ta gorunsun
    options.PopupShowTrivial = true;//cok kisa suren, onemsiz kabul edilen islemleri de goster kapali
    options.PopupShowTimeWithChildren = true;//bir islem suresinin alt islemleri ile birlikte mi gosterilecegini belirler.

    // Profil oturumlarini kullaniciya gore ayirt edebilmek icin UserIdProvider kullan
    options.UserIdProvider = request => request.HttpContext.User.Identity?.Name ?? "anonymous";
    options.ResultsAuthorize = request => true;//Bu delegate, miniporfiler sonuclarini gosteren endpointlere yapilan isteklerde erisim kontrolu yapar.
    options.ResultsListAuthorize = request => true;// bu da toplu sonuc listesini kimin goruntuleyebilecegini belirler.
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseRouting();

app.UseStaticFiles();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs/", options =>//OpenApi endpointlerinizi test edecegin adres
    {
        options.WithTitle("Web App For Mini Profiler Documents")
            .WithTheme(ScalarTheme.DeepSpace);
    });
    app.UseMiniProfiler();
}
app.MapGet("/custom-profiler", async context =>
{
    var filePath = Path.Combine(env.WebRootPath!, "mini", "custom-results-new.html");
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(filePath);
});
app.Use(async (context, next) =>
{
    string ip = "localhost";
    if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
    {
        ip = forwardedFor.ToString()
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .FirstOrDefault()?.Trim() ?? ip;
    }
    else if (context.Connection.RemoteIpAddress is not null)
    {
        ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
    if (MiniProfiler.Current is not null)
    {
        MiniProfiler.Current.MachineName = ip;
    }
    await next();//bu middleware'de ip'yi alir ve profiler.MachineName'e atar
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
