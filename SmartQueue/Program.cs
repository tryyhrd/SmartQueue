using SmartQueue.Data.Interfaces;
using SmartQueue.Data.Mocks;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<SmartQueueContext>(options =>
//    options.UseMySql(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
//        options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
//        )
//    .EnableDetailedErrors()
//    );

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(SmartQueue.Controllers.ServiceController).Assembly);

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IService, MockService>();

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

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Service}/{action=List}/{id?}");

app.MapRazorPages();

app.Run();