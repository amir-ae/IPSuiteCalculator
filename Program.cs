using System.Net;
using System.Net.Sockets;
using IPSuiteCalculator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromDays(1);
    options.Cookie.IsEssential = true;
});

// server IP Address
builder.Configuration["IPAddress"] = Dns.GetHostEntry(Dns.GetHostName())
    .AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseSession();

app.UseMiddleware<ServerMiddleware>();

app.UseRouting();

app.UseEndpoints(endpoints => {
    endpoints.MapFallback(async context =>
        await context.Response.WriteAsync($"Server set up on {app.Configuration["IPAddress"]}:{app.Configuration["Port"]}"));
});

app.Run();
