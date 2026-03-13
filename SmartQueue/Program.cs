using Microsoft.EntityFrameworkCore;
using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Common;
using SmartQueue.Data.Services;

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

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.Listen(System.Net.IPAddress.Any, 5000);
//});

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
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Autorization}/{id?}");

app.MapRazorPages();

app.Run();