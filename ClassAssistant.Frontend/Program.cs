using ClassAssistant.Frontend.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HttpClient pointing to Backend Web API
string backendUrl = builder.Configuration["BackendUrl"] ?? "http://localhost:5000";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(backendUrl) });

var app = builder.Build();

// Configure HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
