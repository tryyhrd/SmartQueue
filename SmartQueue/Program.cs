using Microsoft.EntityFrameworkCore;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Common;
using SmartQueue.Data.Services;
using SmartQueue.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartQueueContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
        )
    .EnableDetailedErrors()
    );

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(SmartQueue.Controllers.ServiceController).Assembly);

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IService, DbService>();
builder.Services.AddTransient<ITicket, DbService>();
builder.Services.AddTransient<IVisitor, DbService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login"; 
        options.LogoutPath = "/Admin/Logout";
        options.AccessDeniedPath = "/Admin/Login";
        options.Cookie.Name = "SmartQueue.AdminAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IpService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); 
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapHub<QueueHub>("/queueHub");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Authorization}/{id?}");
app.MapRazorPages();

app.Run();