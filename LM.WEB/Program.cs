using Blazored.LocalStorage;
using LM.Models.Shared;
using LM.WEB.Commons;
using LM.WEB.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình múi giờ mặc định cho ứng dụng là múi giờ Việt Nam (GMT+7)
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
    options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
});

// config url api
string apiUrl = builder.Configuration.GetSection("appSettings:ApiUrl").Value + "";
string tokenKey = builder.Configuration.GetSection("appSettings:TokenKey").Value + "";
string tokenValue = builder.Configuration.GetSection("appSettings:TokenValue").Value + "";

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<IDateTimeService, VietnamDateTimeService>();
builder.Services.AddTelerikBlazor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddRegisterComponents();

builder.Services.AddClientScopeService();
builder.Services.AddClientAuthorization();
builder.Services.AddHttpClient("api", m =>
{
    m.BaseAddress = new Uri(apiUrl);
    m.Timeout = TimeSpan.FromMinutes(10);
    m.DefaultRequestHeaders.Add(tokenKey, tokenValue);
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
