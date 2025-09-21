using AnimeFeedManager.Features.Infrastructure;
using AnimeFeedManager.Services.Shared;
using AnimeFeedManager.Web.Bootstrapping;
using AnimeFeedManager.Web.Features;
using AnimeFeedManager.Web.Features.Admin.Endpoints;
using AnimeFeedManager.Web.Features.Tv.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.RegisterStorageServices();

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddHttpContextAccessor();
builder.RegisterSecurityServices();
builder.Services.RegisterStorageBasedServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Custom middlewares to add HTMX feature to the request and control behavior of HX- related request
app.UseHtmx();

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();



app.MapRazorComponents<App>();

var endpointsGroup = app
    .MapGroup(string.Empty);

endpointsGroup.MapSecurityEndpoints();
endpointsGroup.MapAdminEndpoints();
endpointsGroup.MapTvSubscriptionsEndpoints();

app.Run();