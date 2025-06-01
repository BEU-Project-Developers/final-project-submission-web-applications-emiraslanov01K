using CaterManagementSystem.Data;
using CaterManagementSystem.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();        // mvc strukturunu avtikl??dirm?y ???nd?r 


builder.Services.AddDbContext<AppDbContext>(options =>  // db il? ?laq? ???n efc konfiqurasiays?d?r 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings")); // mail il? olan ?m?liyyatlar ???n ayarlar 
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)  // asa??dak? ?m?liyyatlar ???n yollar? t?min edir
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout"; 
        options.AccessDeniedPath = "/Account/AccessDenied"; 
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
        options.SlidingExpiration = true; 
    });


builder.Services.AddAuthorization(options =>  // admin v? user ???n giri? qaydalar?n? t?nin edir
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    
});


var app = builder.Build();


using (var scope = app.Services.CreateScope())   // avtomatik db nin migrationlar?n? t?min edir
{
    var services = scope.ServiceProvider; // migration zaman? bir s?hv olarasa ( y?kl?m? v? ya yeni m?lumatlar?n update olmas? kimi bu zaman error verir 
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); 
  
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}



if (!app.Environment.IsDevelopment())  // i?l?m? ax?n?n? nizamlay?r 
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization(); 



 app.MapControllerRoute(  // admin page ???n routing qaydas? 
      name: "areas",
      pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}"
    );

app.MapControllerRoute(   // main page ???n routing qaydas? 
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();