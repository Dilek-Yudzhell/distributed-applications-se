var builder = WebApplication.CreateBuilder(args);

// ==========================================
// RAZOR PAGES
// ==========================================
builder.Services.AddRazorPages();

// ==========================================
// HTTP CLIENT - FOOD ORDERING API
// ==========================================
builder.Services.AddHttpClient("FoodOrderingAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7297/");
});

// ==========================================
// SESSION
// ==========================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// BUILD APPLICATION
// ==========================================
var app = builder.Build();

// ==========================================
// ERROR HANDLING
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// ==========================================
// HTTPS
// ==========================================
app.UseHttpsRedirection();

// ==========================================
// STATIC FILES
// ==========================================
app.UseStaticFiles();

// ==========================================
// ROUTING
// ==========================================
app.UseRouting();

// ==========================================
// SESSION
// ==========================================
app.UseSession();

// ==========================================
// RAZOR PAGES
// ==========================================
app.MapRazorPages();

app.Run();