using ABCRetail.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Azure Storage services
builder.Services.AddSingleton<TableStorageService>();

// Add BlobStorageService
builder.Services.AddSingleton<BlobStorageService>();

// Add QueueStorageService
builder.Services.AddSingleton<QueueStorageService>();

//Add FileStorageService
builder.Services.AddSingleton<FileStorageService>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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