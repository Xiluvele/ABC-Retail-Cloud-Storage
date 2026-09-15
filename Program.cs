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

builder.Services.AddHttpClient<AzureFunctionService>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var baseUrl = configuration["AzureFunctions:BaseUrl"];
    var functionKey = configuration["AzureFunctions:Key"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException(
            "Azure Functions BaseUrl is not configured.");

    client.BaseAddress = new Uri(baseUrl);

    if (!string.IsNullOrWhiteSpace(functionKey))
    {
        client.DefaultRequestHeaders.Add(
            "x-functions-key",
            functionKey);
    }
});

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