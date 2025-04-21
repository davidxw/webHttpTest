using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using webHttpTest.api;
using webHttpTest.Hubs;
using webHttpTest.Models;
using webHttpTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddSingleton<INetworkService, NetworkService>();
builder.Services.AddSingleton<IHostingEnvironmentService, HostingEnvironmentService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<TraceRtHub>("/traceRtHub");

app.MapGet("/api/environment", (IHostingEnvironmentService hostingEnvironmentService) =>
{
    Console.WriteLine($"{DateTime.Now} - Recieved HostEnvironment request");

    return hostingEnvironmentService.PrintHostingEnvironment();
});

app.MapGet("/api/work", (int? duration, int? cpu) =>
{
    Console.WriteLine($"{DateTime.Now} - Recieved work request - CPU: {cpu.Value}% CPU, duration: {duration.Value}");

    return Api.Work(duration, cpu);
});

app.MapGet("/api/get", async (string url) =>
{
    Console.WriteLine($"{DateTime.Now} - Recieved get request - URL: {url}");

    var httpClientHandler = new HttpClientHandler();
    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
    {
        return true;
    };

    var httpClient = new HttpClient(httpClientHandler);
    var result = string.Empty;

    try
    {
        result = await httpClient.GetStringAsync(url);
    }
    catch (HttpRequestException httpex)
    {
        Console.WriteLine(httpex);
        result = $"HttpRequestException: {httpex.Message}";
    }

    return result;
});

app.Map("/api/echo/{*path}", async (HttpRequest httpRequest) =>
{
    Console.WriteLine($"{DateTime.Now} - Recieved echo request for {httpRequest.Path}");

    return await httpRequest.ToEchoResponseAsync();
});

app.MapGet("/api/ping", () =>
{
    return "Hello";
});

app.MapGet("/api/health", () =>
{
    return "Hello";
});

app.MapGet("/api/healthz", () =>
{
    return "Hello";
});

app.Run();
