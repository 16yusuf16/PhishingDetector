using PhishingDetector.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<PhishingApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000");
    client.Timeout = TimeSpan.FromSeconds(300);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.MapGet("/health", () => Results.Ok("Healthy"));
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<PhishingDetector.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
