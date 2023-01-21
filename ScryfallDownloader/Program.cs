using Microsoft.EntityFrameworkCore;
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
builder.Services.AddHttpClient<CardDownloaderService>();
builder.Services.AddHttpClient<DataDownloaderService>();
builder.Services.AddHttpClient<StarCityGamesScraper>();
builder.Services.AddHttpClient<MoxfieldDownloaderService>();
builder.Services.AddHttpClient<MtgTop8DownloaderService>();
builder.Services.AddSingleton<IOService>();
builder.Services.AddSingleton<ImageService>();
builder.Services.AddSingleton<ForgeService>();
builder.Services.AddSingleton<DataService>();

builder.Services.AddDbContextFactory<DatabaseContext>(options =>
{
    options.UseSqlite("Data Source=database.db");
});

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
