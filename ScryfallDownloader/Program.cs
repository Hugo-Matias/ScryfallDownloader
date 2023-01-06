using ScryfallApi.Client;
using ScryfallDownloader.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddHttpClient<ScryfallApiService>();
builder.Services.AddHttpClient<ScryfallApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.scryfall.com");
});
builder.Services.AddHttpClient<DownloaderService>();
builder.Services.AddSingleton<IOService>();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<ForgeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
