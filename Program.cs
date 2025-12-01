using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RazorPagesUser.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<RazorPagesUserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RazorPagesUserContext") ?? throw new InvalidOperationException("Connection string 'RazorPagesUserContext' not found.")));

var app = builder.Build();

// Test database connection
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RazorPagesUserContext>();
    try
    {
        await context.Database.CanConnectAsync();
        Console.WriteLine("[+] Database connection successful!");
    }
    catch
    {
        Console.WriteLine($"[-] Database connection failed.");
    }
}


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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
