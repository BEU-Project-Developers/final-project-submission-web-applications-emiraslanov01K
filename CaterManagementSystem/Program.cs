using CaterManagementSystem.Models;
using CaterManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// DbContext konfiqurasiyas?
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // "DefaultConnection" ad?n? appsettings.json-dak? il? uy?unla?d?r?n

// Email servisi üçün konfiqurasiya
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Cookie ?sasl? autentifikasiya konfiqurasiyas?
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Login s?hif?sinin ünvan?
        options.LogoutPath = "/Account/Logout"; // Logout ünvan?
        options.AccessDeniedPath = "/Account/AccessDenied"; // ?caz? olmad?qda yönl?ndiril?c?k s?hif?
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Cookie-nin etibarl?l?q müdd?ti
        options.SlidingExpiration = true; // H?r sor?uda etibarl?l?q müdd?tini yenil?
    });

// Avtorizasiya siyas?tl?ri (laz?m olarsa)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    // Dig?r siyas?tl?ri burada t?yin ed? bil?rsiniz
});


var app = builder.Build();

// Veril?nl?r bazas? miqrasiyalar?n? t?tbiq etm?k v? ilkin rollar? ?lav? etm?k üçün (istehsalat üçün daha yax?? bir yol dü?ünün)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Miqrasiyalar? t?tbiq edir
        // ?lkin rollar?n AppDbContext-d? HasData il? ?lav? olundu?undan ?min olun
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Vacibdir! Autentifikasiyan? aktivl??dirir
app.UseAuthorization();  // Vacibdir! Avtorizasiyan? aktivl??dirir

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();