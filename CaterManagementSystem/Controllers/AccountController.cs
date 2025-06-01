// CaterManagementSystem.Controllers/AccountController.cs
using CaterManagementSystem.Models;
using CaterManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CaterManagementSystem.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using CaterManagementSystem.Data;
using System;

namespace CaterManagementSystem.Controllers
{
    [AllowAnonymous] // Bütün action-lar default olaraq anonim olmasını təmin etmək 
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(AppDbContext context,
                                 IEmailSender emailSender,
                                 IPasswordService passwordService,
                                 ILogger<AccountController> logger,
                                 IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _passwordService = passwordService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        //  register və email təstiqləmə
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
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
                    FullName = model.FullName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    EmailConfirmed = false,
                    RegistrationDate = DateTime.UtcNow,
                    ConfirmationToken = Guid.NewGuid().ToString("N"),
                    ConfirmationTokenExpiryDate = DateTime.UtcNow.AddDays(1)
                };

                var defaultRole = await _context.Role.FirstOrDefaultAsync(r => r.Name == "User");
                if (defaultRole == null)
                {
                    _logger.LogError("Default 'User' role not found. Please seed the database.");
                    ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı sistem xətası. Administrator ilə əlaqə saxlayın.");
                    return View(model);
                }
                user.UserRoles.Add(new UserRole { Role = defaultRole });

                var userDetails = new UserDetails
                {
                    User = user,
                    ImagePath = "default-avatar.png"
                };

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        _context.Users.Add(user);
                        _context.UserDetails.Add(userDetails);
                        await _context.SaveChangesAsync();

                        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                                           new { userId = user.Id, token = user.ConfirmationToken },
                                           protocol: Request.Scheme);

                        if (!string.IsNullOrEmpty(callbackUrl))
                        {
                            await _emailSender.SendEmailAsync(model.Email, "Hesabınızı Təsdiqləyin",
                                $"Zəhmət olmasa hesabınızı <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>buraya</a> klikləyərək təsdiqləyin.");
                            _logger.LogInformation("Confirmation email sent to {Email}", model.Email);
                        }
                        else
                        {
                            _logger.LogWarning("Could not generate callback URL for email confirmation for user {Email}.", model.Email);
                        }
                        await transaction.CommitAsync();
                        return RedirectToAction(nameof(RegisterConfirmation), new { email = model.Email });
                    }
                    catch (DbUpdateException dbEx)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(dbEx, "Database error during registration for {Email}. InnerException: {InnerEx}", model.Email, dbEx.InnerException?.Message);
                        ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı verilənlər bazası xətası. Məlumatların unikallığını yoxlayın.");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "General error during registration for {Email}. Details: {ExceptionDetails}", model.Email, ex.ToString());
                        ModelState.AddModelError(string.Empty, "Qeydiyyat zamanı xəta baş verdi. Zəhmət olmasa yenidən cəhd edin.");
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult RegisterConfirmation(string? email)
        {
            ViewBag.Email = email;
            ViewBag.Message = string.IsNullOrEmpty(email)
                ? "Qeydiyyatınız demək olar ki, tamamlandı. E-poçt ünvanınıza təsdiqləmə linki göndərməyə çalışdıq."
                : $"Hesabınızı aktivləşdirmək üçün {email} ünvanına təsdiqləmə linki göndərdik. Zəhmət olmasa e-poçt qutunuzu (spam/junk qovluğunu da) yoxlayın.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(int? userId, string? token)
        {
            if (userId == null || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Yanlış təsdiqləmə linki.";
                return RedirectToAction("Index", "Home");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.ConfirmationToken == token);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    ViewBag.Message = "E-poçt ünvanınız artıq təsdiqlənib.";
                }
                else if (user.ConfirmationTokenExpiryDate.HasValue && user.ConfirmationTokenExpiryDate.Value < DateTime.UtcNow)
                {
                    ViewBag.Message = "Təsdiqləmə linkinin vaxtı bitib.";
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
            else
            {
                ViewBag.Message = "Yanlış və ya etibarsız təsdiqləmə linki.";
            }
            return View();
        }

        //  login hissəsi
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
                                         .Include(u => u.UserRoles!)
                                             .ThenInclude(ur => ur.Role)
                                         .Include(u => u.UserDetails)
                                         .FirstOrDefaultAsync(u => u.UserName == model.EmailOrUserName || u.Email == model.EmailOrUserName);

                if (user != null && _passwordService.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
                {
                    if (!user.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "Daxil olmaq üçün e-poçt ünvanınızı təsdiqləməlisiniz.");
                        return View(model);
                    }
                    await SignInUserAsync(user, model.RememberMe);
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
                new Claim(ClaimTypes.GivenName, user.FullName ?? string.Empty)
            };
            string profilePicturePath = user.UserDetails?.ImagePath ?? "default-avatar.png";
            claims.Add(new Claim("ProfilePicture", profilePicturePath));
            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }
                }
            }
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties { IsPersistent = isPersistent };
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        // forget password və create new passord hissələri
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
                    Random random = new Random();
                    string verificationCode = random.Next(100000, 999999).ToString();

                    user.PasswordResetCode = verificationCode; // Yeni sahəni istifadə edirik
                    user.PasswordResetCodeExpiryDate = DateTime.UtcNow.AddMinutes(15);
                    await _context.SaveChangesAsync();

                    try
                    {
                        await _emailSender.SendEmailAsync(model.Email, "Şifrə Sıfırlama Kodu",
                            $"Şifrənizi sıfırlamaq üçün təsdiq kodunuz: <strong>{verificationCode}</strong><br>Bu kod 15 dəqiqə ərzində etibarlıdır.");
                        TempData["InfoMessage"] = "E-poçt ünvanınıza şifrə sıfırlama kodu göndərildi.";
                        return RedirectToAction(nameof(EnterResetCode), new { email = model.Email });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending password reset code to {Email}. Details: {ExceptionDetails}", model.Email, ex.ToString());
                        TempData["ErrorMessage"] = "Kod göndərilərkən xəta baş verdi.";
                    }
                }
                else
                {
                    TempData["InfoMessage"] = "Əgər daxil etdiyiniz e-poçt ünvanı sistemdə qeydiyyatdan keçmiş və təsdiqlənmişdirsə, sıfırlama kodu göndəriləcək.";
                }
            
                if (user == null || !user.EmailConfirmed)
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                // Əgər kod göndərilməyibsə (catch bloku), yenə ForgotPasswordConfirmation.
                if (TempData["ErrorMessage"] != null && !TempData.ContainsKey("InfoMessage"))
                {
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }
                // Əgər kod uğurla göndərilibsə, EnterResetCode-a yönləndirilir (yuxarıdakı try blokunda).
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
           
            return View();
        }


        [HttpGet]
        public IActionResult EnterResetCode(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "E-poçt ünvanı təyin edilməyib.";
                return RedirectToAction(nameof(ForgotPassword));
            }
            var model = new EnterResetCodeViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnterResetCode(EnterResetCodeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email &&
                                                                         u.PasswordResetCode == model.Code &&
                                                                         u.PasswordResetCodeExpiryDate.HasValue &&
                                                                         u.PasswordResetCodeExpiryDate.Value > DateTime.UtcNow);
                if (user != null)
                {
                   
                    return RedirectToAction(nameof(ResetPassword), new { email = model.Email, code = model.Code });
                }
                else
                {
                    ModelState.AddModelError(nameof(model.Code), "Təsdiq kodu yanlışdır, vaxtı bitib və ya belə bir tələb mövcud deyil.");
                }
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string? email, string? code)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "Şifrə sıfırlama tələbi üçün e-poçt və ya kod tapılmadı.";
                return RedirectToAction(nameof(ForgotPassword));
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email &&
                                                                     u.PasswordResetCode == code &&
                                                                     u.PasswordResetCodeExpiryDate.HasValue &&
                                                                     u.PasswordResetCodeExpiryDate.Value > DateTime.UtcNow);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Şifrə sıfırlama üçün istifadə edilən kod artıq etibarsızdır. Zəhmət olmasa yenidən cəhd edin.";
                return RedirectToAction(nameof(EnterResetCode), new { email = email });
            }
            var viewModel = new ResetPasswordViewModel { Email = email, Code = code };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email &&
                                                                         u.PasswordResetCode == model.Code &&
                                                                         u.PasswordResetCodeExpiryDate.HasValue &&
                                                                         u.PasswordResetCodeExpiryDate.Value >= DateTime.UtcNow);
                if (user != null)
                {
                    _passwordService.CreatePasswordHash(model.Password, out byte[] newPasswordHash, out byte[] newPasswordSalt);
                    user.PasswordHash = newPasswordHash;
                    user.PasswordSalt = newPasswordSalt;
                    user.PasswordResetCode = null; // Kodu istifadə etdikdən sonra sıfırlama hissəsi 
                    user.PasswordResetCodeExpiryDate = null;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Şifrəniz uğurla yeniləndi. İndi daxil ola bilərsiniz.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Şifrə sıfırlama cəhdi uğursuz oldu. Kod etibarsız və ya vaxtı bitib.");
                }
            }
            return View(model);
        }

        // update profile 
        [Authorize] // Bu action yalnız daxil olmuş istifadəçilər üçün
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized();
            }
            var user = await _context.Users.Include(u => u.UserDetails).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();
            var viewModel = new EditProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                CurrentImagePath = user.UserDetails?.ImagePath
            };
            return View(viewModel);
        }

        [Authorize] // Bu action yalnız daxil olmuş istifadəçilər üçün
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId) || model.UserId != currentUserId)
            {
                return Forbid();
            }
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.CurrentImagePath) && model.UserId > 0)
                {
                    var userForImage = await _context.Users.Include(u => u.UserDetails)
                                                      .AsNoTracking()
                                                      .FirstOrDefaultAsync(u => u.Id == model.UserId);
                    model.CurrentImagePath = userForImage?.UserDetails?.ImagePath;
                }
                return View(model);
            }
            var user = await _context.Users.Include(u => u.UserDetails).FirstOrDefaultAsync(u => u.Id == model.UserId);
            if (user == null) return NotFound();
            bool claimsNeedRefresh = false;

            if (user.UserName != model.UserName)
            {
                if (await _context.Users.AnyAsync(u => u.UserName == model.UserName && u.Id != user.Id))
                {
                    ModelState.AddModelError(nameof(model.UserName), "Bu istifadəçi adı artıq mövcuddur.");
                }
                else
                {
                    user.UserName = model.UserName;
                    claimsNeedRefresh = true;
                }
            }
            if (user.Email != model.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != user.Id))
                {
                    ModelState.AddModelError(nameof(model.Email), "Bu e-poçt ünvanı artıq başqa hesab tərəfindən istifadə olunur.");
                }
                else
                {
                    user.Email = model.Email;
                    claimsNeedRefresh = true;
                }
            }
            if (!ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(model.CurrentImagePath) && user.UserDetails != null) model.CurrentImagePath = user.UserDetails.ImagePath;
                return View(model);
            }
            user.FullName = model.FullName;
            if ((User.FindFirstValue(ClaimTypes.GivenName) ?? "") != (user.FullName ?? "")) claimsNeedRefresh = true;

            if (user.UserDetails == null)
            {
                user.UserDetails = new UserDetails { UserId = user.Id, ImagePath = "default-avatar.png" };
            }
            if (model.Photo != null && model.Photo.Length > 0)
            {
                string oldImageRelativePath = user.UserDetails.ImagePath ?? "";
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profile_pictures");
                Directory.CreateDirectory(uploadsFolder);
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileNameWithoutExtension(model.Photo.FileName) + Path.GetExtension(model.Photo.FileName);
                string newFilePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fileStream);
                }
                string newImageRelativePath = Path.Combine("uploads", "profile_pictures", uniqueFileName).Replace("\\", "/");
                user.UserDetails.ImagePath = newImageRelativePath;
                claimsNeedRefresh = true;
                if (!string.IsNullOrEmpty(oldImageRelativePath) && oldImageRelativePath != "default-avatar.png" && oldImageRelativePath != newImageRelativePath)
                {
                    string oldFilePathFull = Path.Combine(_webHostEnvironment.WebRootPath, oldImageRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePathFull))
                    {
                        try { System.IO.File.Delete(oldFilePathFull); }
                        catch (IOException ex) { _logger.LogWarning(ex, "Could not delete old profile picture: {Path}", oldFilePathFull); }
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Yeni şifrə təyin etmək üçün hazırkı şifrənizi daxil etməlisiniz.");
                }
                else if (!_passwordService.VerifyPasswordHash(model.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Hazırkı şifrə yanlışdır.");
                }
                if (!ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.CurrentImagePath) && user.UserDetails != null) model.CurrentImagePath = user.UserDetails.ImagePath;
                    return View(model);
                }
                _passwordService.CreatePasswordHash(model.NewPassword, out byte[] newHash, out byte[] newSalt);
                user.PasswordHash = newHash;
                user.PasswordSalt = newSalt;
                claimsNeedRefresh = true;
            }
            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil məlumatlarınız uğurla yeniləndi.";
                if (claimsNeedRefresh)
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await SignInUserAsync(user, User.Identity?.IsAuthenticated ?? false);
                }
                return RedirectToAction(nameof(EditProfile));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error editing profile for {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "Məlumatlar eyni anda başqası tərəfindən dəyişdirilib. Zəhmət olmasa səhifəni yeniləyib təkrar cəhd edin.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing profile for {UserId}. Details: {ExceptionDetails}", model.UserId, ex.ToString());
                ModelState.AddModelError(string.Empty, "Profil redaktə edilərkən xəta baş verdi.");
            }
            if (string.IsNullOrEmpty(model.CurrentImagePath) && user.UserDetails != null) model.CurrentImagePath = user.UserDetails.ImagePath;
            return View(model);
        }

        // logout və not allowed hisseleri 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Sistemdən uğurla çıxış etdiniz.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}