using AnimeFeedManager.Web.Bootstrapping;
using AnimeFeedManager.Web.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddHttpContextAccessor();
builder.RegisterSecurityServices();
builder.RegisterAppServices();

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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

var endpointsGroup = app
    .MapGroup(string.Empty);

endpointsGroup.MapSecurityEndpoints();

app.Run();