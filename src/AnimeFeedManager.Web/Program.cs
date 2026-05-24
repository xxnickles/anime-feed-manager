using AnimeFeedManager.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebAppDefaults();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapGet("/", () => "v5 booting");

app.Run();
