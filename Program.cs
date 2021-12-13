using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using webHttpTest.api;
using webHttpTest.Hubs;
using webHttpTest.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddSingleton<INetworkService, NetworkService>();
builder.Services.AddSingleton<IHostingEnvironmentService, HostingEnvironmentService>();

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

app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<TraceRtHub>("/traceRtHub");
});

// quick and dirty api

app.MapGet("/api/environment", (IHostingEnvironmentService hostingEnvironmentService) =>
{
    return hostingEnvironmentService.PrintHostingEnvironment();
});

app.MapGet("/api/work", (int ? duration, int ? cpu) =>
{
    return Api.Work(duration, cpu);
});

app.Run();


