var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("Backend", c =>
{
    c.BaseAddress = new Uri("http://localhost:5265/api/");
});

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();