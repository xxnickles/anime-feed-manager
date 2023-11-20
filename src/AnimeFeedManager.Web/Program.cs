using AnimeFeedManager.Web.Bootstrapping;
using AnimeFeedManager.Web.Features;
using Microsoft.AspNetCore.Components.Web;
using TvEndpoints = AnimeFeedManager.Web.Features.Tv.Endpoints; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

// Add the renderer and wrapper to services
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<BlazorRenderer>();

// Application dependencies
builder.Services.RegisterAppDependencies(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

TvEndpoints.Map(app);

app.Run();
