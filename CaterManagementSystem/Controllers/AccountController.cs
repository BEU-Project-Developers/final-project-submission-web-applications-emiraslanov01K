// Controllers/AccountController.cs
using CaterManagementSystem.Models;
using CaterManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CaterManagementSystem.Services; // IEmailSender və IPasswordService üçün
using System.Text.Encodings.Web; // HtmlEncoder üçün
using Microsoft.AspNetCore.Authorization; // [Authorize] üçün
using Microsoft.AspNetCore.Hosting; // IWebHostEnvironment üçün (EditProfile-da şəkil yükləmə)
using System.IO;
using SchoolSystem.Models; // Path, FileStream üçün

namespace CaterManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // Profil şəkli üçün

        public AccountController(AppDbContext context,
                                 IEmailSender emailSender,
                                 IPasswordService passwordService,
                                 ILogger<AccountController> logger,
                                 IWebHostEnvironment webHostEnvironment) // Əlavə edildi
        {
            _context = context;
            _emailSender = emailSender;
            _passwordService = passwordService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; // Təyin edildi
        }

        // === QEYDİYYAT VƏ E-POÇT TƏSDİQLƏMƏ ===

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) // RegisterViewModel-də FullName sahəsi olmalıdır
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Bu istifadəçi adı artıq mövcuddur.");
                }
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "Bu e-poçt ünvanı artıq qeydiyyatdan keçib.");
                }

                if (!ModelState.IsValid) return View(model);

                _passwordService.CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName, // RegisterViewModel-dən gəlməlidir
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    EmailConfirmed = false,
                    RegistrationDate = DateTime.UtcNow,
                    ConfirmationToken = Guid.NewGuid().ToString("N"),
                    ConfirmationTokenExpiryDate = DateTime.UtcNow.AddDays(1),
                    ImagePath = "default-avatar.png" // Varsayılan şəkil
                };

                // Varsayılan "User" rolunu təyin et
                var defaultRole = await _context.Role.FirstOrDefaultAsync(r => r.Name == "User");
                if (defaultRole == null)
                {
                    _logger.LogError("Default 'User' role not found. Please seed the database.");
                    ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı sistem xətası. Administrator ilə əlaqə saxlayın.");
                    return View(model);
                }
                user.UserRoles.Add(new UserRole { Role = defaultRole }); // User-ə birbaşa əlavə etdik

                // İlkin UserDetails yarat
                var userDetails = new UserDetails
                {
                    User = user, // User obyektini birbaşa təyin et
                    // Username və Fullname UserDetails-dən çıxarılıbsa, bu sətirlər lazım deyil
                    // Username = user.UserName,
                    // Fullname = user.FullName,
                    ImagePath = user.ImagePath // User-dəki default şəkil ilə başla
                };
                // user.UserDetails = userDetails; // Bu şəkildə də əlaqə qurula bilər və ya birbaşa context-ə əlavə edilə bilər
                // EF Core əlaqələri avtomatik idarə edəcək əgər UserDetails.User təyin olunubsa.

                _context.Users.Add(user);
                _context.UserDetails.Add(userDetails); // UserDetails-i də context-ə əlavə et

                try
                {
                    await _context.SaveChangesAsync();

                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                                       new { userId = user.Id, token = user.ConfirmationToken },
                                       protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Hesabınızı Təsdiqləyin",
                        $"Zəhmət olmasa hesabınızı <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>buraya</a> klikləyərək təsdiqləyin.");

                    _logger.LogInformation("Confirmation email sent to {Email}", model.Email);
                    return RedirectToAction(nameof(RegisterConfirmation));
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error during registration for {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı verilənlər bazası xətası. Məlumatların unikallığını yoxlayın.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı xəta baş verdi. Zəhmət olmasa yenidən cəhd edin.");
                    // Əgər istifadəçi qismən yaradılıbsa, onu geri almaq (rollback) mürəkkəb ola bilər.
                    // Ən yaxşısı, transaction istifadə etməkdir, amma sadəlik üçün hələlik belə saxlayırıq.
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation() => View();

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int? userId, string? token)
        {
            if (userId == null || token == null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.ConfirmationToken == token);

            if (user != null && user.ConfirmationTokenExpiryDate >= DateTime.UtcNow)
            {
                if (user.EmailConfirmed)
                {
                    ViewBag.Message = "E-poçt ünvanınız artıq təsdiqlənib.";
                }
                else
                {
                    user.EmailConfirmed = true;
                    user.ConfirmationToken = null;
                    user.ConfirmationTokenExpiryDate = null;
                    await _context.SaveChangesAsync();
                    ViewBag.Message = "E-poçt ünvanınız uğurla təsdiqləndi! İndi daxil ola bilərsiniz.";
                }
            }
            else if (user != null && user.ConfirmationTokenExpiryDate < DateTime.UtcNow)
            {
                ViewBag.Message = "Təsdiqləmə linkinin vaxtı bitib.";
            }
            else
            {
                ViewBag.Message = "Yanlış təsdiqləmə linki.";
            }
            return View();
        }

        // === DAXİL OLMA ===
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                                         .Include(u => u.UserRoles!) // UserRoles null ola bilməz deyə ! işarəsi (əgər əmin deyilsinizsə, yoxlayın)
                                             .ThenInclude(ur => ur.Role)
                                         .Include(u => u.UserDetails) // UserDetails-i də yüklə
                                         .FirstOrDefaultAsync(u => u.Email == model.EmailOrUserName || u.UserName == model.EmailOrUserName);

                if (user != null && _passwordService.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Zəhmət olmasa e-poçt ünvanınızı təsdiqləyin.");
                        return View(model);
                    }

                    await SignInUserAsync(user, model.RememberMe); // SignInUser metodunu çağır

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Yanlış istifadəçi adı/e-poçt və ya şifrə.");
            }
            return View(model);
        }

        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FullName ?? string.Empty) // FullName null ola bilər deyə ??
            };

            // Profil şəklini UserDetails-dən (əgər varsa) və ya User-dən al
            string profilePicturePath = user.UserDetails?.ImagePath ?? user.ImagePath ?? "default-avatar.png";
            claims.Add(new Claim("ProfilePicture", profilePicturePath));


            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null) // Role null ola bilər deyə yoxlama
                {
                    claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }


        // === ŞİFRƏNİ UNUTDUM / ŞİFRƏ SIFIRLAMA ===
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null && user.EmailConfirmed)
                {
                    user.PasswordResetToken = Guid.NewGuid().ToString("N");
                    user.PasswordResetTokenExpiryDate = DateTime.UtcNow.AddHours(2);
                    await _context.SaveChangesAsync();

                    try
                    {
                        var callbackUrl = Url.Action(nameof(ResetPassword), "Account",
                                           new { email = user.Email, token = user.PasswordResetToken },
                                           protocol: Request.Scheme);
                        await _emailSender.SendEmailAsync(model.Email, "Şifrə Sıfırlama Sorğusu",
                            $"Şifrənizi sıfırlamaq üçün <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>buraya</a> klikləyin.");
                        TempData["InfoMessage"] = "Şifrə sıfırlama təlimatları e-poçt ünvanınıza göndərildi.";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending password reset email to {Email}", model.Email);
                        TempData["ErrorMessage"] = "E-poçt göndərilərkən xəta baş verdi.";
                    }
                }
                else
                {
                    // İstifadəçinin mövcudluğunu bildirməmək daha təhlükəsizdir
                    TempData["InfoMessage"] = "Əgər daxil etdiyiniz e-poçt ünvanı sistemdə mövcuddursa, sıfırlama təlimatları göndəriləcək.";
                }
                return View(model); // Formu yenidən göstər, mesaj TempData-da olacaq
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? email, string? token)
        {
            if (email == null || token == null)
            {
                TempData["ErrorMessage"] = "Yanlış şifrə sıfırlama linki.";
                return RedirectToAction(nameof(Login));
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordResetToken == token);
            if (user == null || user.PasswordResetTokenExpiryDate < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] = "Şifrə sıfırlama linki etibarsızdır və ya vaxtı bitib.";
                return RedirectToAction(nameof(Login));
            }
            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordResetToken == model.Token);
                if (user != null && user.PasswordResetTokenExpiryDate >= DateTime.UtcNow)
                {
                    _passwordService.CreatePasswordHash(model.Password, out byte[] newPasswordHash, out byte[] newPasswordSalt);
                    user.PasswordHash = newPasswordHash;
                    user.PasswordSalt = newPasswordSalt;
                    user.PasswordResetToken = null;
                    user.PasswordResetTokenExpiryDate = null;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Şifrəniz uğurla yeniləndi.";
                    return RedirectToAction(nameof(Login));
                }
                TempData["ErrorMessage"] = "Şifrə sıfırlama cəhdi uğursuz oldu. Link etibarsız və ya vaxtı bitib.";
            }
            return View(model);
        }

        // === PROFİL REDAKTƏ ===
        [Authorize] // Yalnız daxil olmuş istifadəçilər üçün
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(); // Və ya BadRequest
            }

            var user = await _context.Users.Include(u => u.UserDetails).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel // EditProfileViewModel yaratmaq lazımdır
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email, // E-poçt redaktəsi mürəkkəbdir, yeni təsdiq tələb edə bilər
                FullName = user.FullName,
                CurrentImagePath = user.UserDetails?.ImagePath ?? user.ImagePath // UserDetails-dən və ya User-dən al
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model) // EditProfileViewModel lazımdır
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId) || model.UserId != currentUserId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                // CurrentImagePath-i modelə yenidən yüklə, çünki Post-da itir
                var userForImage = await _context.Users.Include(u => u.UserDetails)
                                                  .AsNoTracking()
                                                  .FirstOrDefaultAsync(u => u.Id == model.UserId);
                model.CurrentImagePath = userForImage?.UserDetails?.ImagePath ?? userForImage?.ImagePath;
                return View(model);
            }

            var user = await _context.Users.Include(u => u.UserDetails).FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null) return NotFound();

            // İstifadəçi adı unikallığını yoxla (əgər dəyişibsə)
            if (user.UserName != model.UserName && await _context.Users.AnyAsync(u => u.UserName == model.UserName && u.Id != user.Id))
            {
                ModelState.AddModelError(nameof(model.UserName), "Bu istifadəçi adı artıq mövcuddur.");
                model.CurrentImagePath = user.UserDetails?.ImagePath ?? user.ImagePath;
                return View(model);
            }
            // E-poçt dəyişikliyi üçün də bənzər yoxlama və yeni təsdiq prosesi lazım ola bilər

            user.UserName = model.UserName;
            user.FullName = model.FullName;
            // user.Email = model.Email; // E-poçt dəyişikliyi üçün ehtiyatlı olun

            if (user.UserDetails == null) // Əgər UserDetails yoxdursa, yarat
            {
                user.UserDetails = new UserDetails { UserId = user.Id };
                _context.UserDetails.Add(user.UserDetails);
            }

            if (model.Photo != null && model.Photo.Length > 0)
            {
                string oldImageRelativePath = user.UserDetails.ImagePath ?? user.ImagePath ?? "";

                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pictures");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Photo.FileName);
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fileStream);
                }
                // Verilənlər bazasına nisbi yolu yaz
                string newImageRelativePath = Path.Combine("uploads", "profile_pictures", uniqueFileName).Replace("\\", "/");
                user.UserDetails.ImagePath = newImageRelativePath;
                user.ImagePath = newImageRelativePath; // User-dəki ImagePath-i də yenilə

                // Köhnə şəkli sil (əgər default deyilsə)
                if (!string.IsNullOrEmpty(oldImageRelativePath) && oldImageRelativePath != "default-avatar.png")
                {
                    string oldFilePathFull = Path.Combine(_webHostEnvironment.WebRootPath, oldImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePathFull))
                    {
                        try { System.IO.File.Delete(oldFilePathFull); }
                        catch (IOException ex) { _logger.LogWarning(ex, "Could not delete old profile picture: {Path}", oldFilePathFull); }
                    }
                }
            }

            // Şifrə dəyişikliyi (əgər EditProfileViewModel-də varsa)
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || !_passwordService.VerifyPasswordHash(model.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Hazırkı şifrə yanlışdır.");
                    model.CurrentImagePath = user.UserDetails?.ImagePath ?? user.ImagePath;
                    return View(model);
                }
                _passwordService.CreatePasswordHash(model.NewPassword, out byte[] newHash, out byte[] newSalt);
                user.PasswordHash = newHash;
                user.PasswordSalt = newSalt;
            }


            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil məlumatlarınız uğurla yeniləndi.";

                // Əgər kritik claim-lər dəyişibsə, istifadəçini yenidən daxil et
                bool claimsChanged = User.FindFirstValue(ClaimTypes.Name) != user.UserName ||
                                     User.FindFirstValue(ClaimTypes.GivenName) != user.FullName ||
                                     (model.Photo != null && User.FindFirstValue("ProfilePicture") != (user.UserDetails?.ImagePath ?? user.ImagePath));

                if (claimsChanged || !string.IsNullOrEmpty(model.NewPassword))
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await SignInUserAsync(user, true); // isPersistent true ola bilər
                }
                return RedirectToAction(nameof(EditProfile));
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogError("Concurrency error while editing profile for user {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "Məlumatlar eyni anda başqası tərəfindən dəyişdirilib. Zəhmət olmasa səhifəni yeniləyib təkrar cəhd edin.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing profile for user {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "Profil redaktə edilərkən xəta baş verdi.");
            }

            model.CurrentImagePath = user.UserDetails?.ImagePath ?? user.ImagePath;
            return View(model);
        }


        // === ÇIXIŞ və İCAZƏ RƏDDİ ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}