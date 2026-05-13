using System;
using System.Web.Mvc;
using Maeen1_New.Models;
using System.Linq;
using System.Configuration;

namespace Maeen1_New.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            try
            {
                if (!IsDatabaseConnectionConfigured())
                {
                    ViewBag.Error = "إعدادات قاعدة البيانات غير مكتملة في Web.config. يرجى ضبط YOUR_PROJECT_REF و YOUR_PASSWORD.";
                    return View();
                }

                using (var db = new Maeen1_NewDbContext())
                {
                    var normalizedEmail = (Email ?? string.Empty).Trim().ToLower();
                    var rawPassword = Password ?? string.Empty;
                    var trimmedPassword = rawPassword.Trim();

                    var user = db.Users
                                 .FirstOrDefault(u =>
                                     u.Email != null &&
                                     u.Email.Trim().ToLower() == normalizedEmail &&
                                     (u.Password == rawPassword || u.Password == trimmedPassword));

                    var normalizedRole = (user != null ? user.Role : null) ?? string.Empty;
                    normalizedRole = normalizedRole.Trim().ToLower();

                    if (user != null && normalizedRole == "student")
                    {
                        return RedirectToAction("Dashboard", "Student", new { userId = user.Id });
                    }
                    else if (user != null && normalizedRole == "teacher")
                    {
                        return RedirectToAction("Dashboard", "Teacher", new { userId = user.Id });
                    }
                    else if (user != null && normalizedRole == "admin")
                    {
                        return RedirectToAction("Dashboard", "Admin", new { userId = user.Id });
                    }
                }

                ViewBag.Error = "بيانات الدخول غير صحيحة";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "تعذر الاتصال بقاعدة البيانات. " + ex.Message;
                return View();
            }

        }
    
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string Name, string Email, string Password, string Role)
        {
            try
            {
                if (!IsDatabaseConnectionConfigured())
                {
                    ViewBag.Error = "إعدادات قاعدة البيانات غير مكتملة في Web.config. يرجى ضبط YOUR_PROJECT_REF و YOUR_PASSWORD.";
                    return View();
                }

                using (var db = new Maeen1_NewDbContext())
                {
                    var normalizedEmail = (Email ?? string.Empty).Trim().ToLower();
                    var normalizedRole = (Role ?? string.Empty).Trim().ToLower();
                    var roleToSave = normalizedRole == "teacher"
                        ? "Teacher"
                        : normalizedRole == "admin"
                            ? "Admin"
                            : "Student";

                    User user = new User
                    {
                        Name = (Name ?? string.Empty).Trim(),
                        Email = normalizedEmail,
                        Password = Password,
                        Role = roleToSave,
                        IsOnboardingCompleted = false
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    if (user.Role == "Student")
                    {
                        return RedirectToAction("Dashboard", "Student", new { userId = user.Id });
                    }
                    else if (user.Role == "Teacher")
                    {
                        return RedirectToAction("Dashboard", "Teacher", new { userId = user.Id });
                    }
                    else if (user.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin", new { userId = user.Id });
                    }
                }

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "تعذر الاتصال بقاعدة البيانات. " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public ActionResult EnsureTestAdmin()
        {
            try
            {
                if (!IsDatabaseConnectionConfigured())
                {
                    return Content("إعدادات قاعدة البيانات غير مكتملة في Web.config.");
                }

                const string adminEmail = "admin.test@maeen.local";
                const string adminPassword = "Admin@123456";

                using (var db = new Maeen1_NewDbContext())
                {
                    var existingAdmin = db.Users.FirstOrDefault(u => u.Email == adminEmail);
                    if (existingAdmin == null)
                    {
                        var admin = new User
                        {
                            Name = "Admin Test",
                            Email = adminEmail,
                            Password = adminPassword,
                            Role = "Admin",
                            IsOnboardingCompleted = true,
                            OnboardingCompletedAt = DateTime.UtcNow
                        };

                        db.Users.Add(admin);
                        db.SaveChanges();
                    }
                }

                return Content("تم تجهيز حساب الأدمن التجريبي: admin.test@maeen.local / Admin@123456");
            }
            catch (Exception ex)
            {
                return Content("تعذر تجهيز حساب الأدمن: " + ex.Message);
            }
        }

        private bool IsDatabaseConnectionConfigured()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["Maeen1ConnectionString"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            return connectionString.IndexOf("YOUR_PROJECT_REF", StringComparison.OrdinalIgnoreCase) < 0
                && connectionString.IndexOf("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase) < 0;
        }
    }

}
