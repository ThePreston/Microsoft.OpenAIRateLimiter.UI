using Azure.Identity;
using Azure.ResourceManager;
using Microsoft.OpenAIRateLimiter.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("QuotaService", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["QuotaAPIUrl"]);
    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", builder.Configuration["QuotaAPIKey"]);

});

builder.Services.AddHttpClient("ChatAPI", httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["QuotaAPIUrl"]);

});

builder.Services.AddScoped(Provider =>
{
    return new ArmClient(new DefaultAzureCredential(true));
});

builder.Services.AddScoped<APIMService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
