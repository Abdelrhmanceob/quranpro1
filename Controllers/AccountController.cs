using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Maeen1_New.Models;
using System.Linq;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Maeen1_New.Controllers
{
    public class AccountController : Controller
    {
        Maeen1_NewDbContext db = new Maeen1_NewDbContext();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            var user = db.Users
                         .FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                // نتحقق هل طالبة أو معلمة
                if (user.Role == "Student")
                {
                    return RedirectToAction("Dashboard", "Student");
                }
                else if (user.Role == "Teacher")
                {
                    return RedirectToAction("Dashboard", "Teacher");
                }
            }

            ViewBag.Error = "بيانات الدخول غير صحيحة";
            return View();

        }
    
    public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string Name, string Email, string Password, string Role)
        {
            User user = new User
            {
                Name = Name,
                Email = Email,
                Password = Password,
                Role = Role
            };

            db.Users.Add(user);
            db.SaveChanges();

            return RedirectToAction("Login");
        }
    }

   
    }
