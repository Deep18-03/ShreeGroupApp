using ShreeGroup.DAL.DbOperations;
using ShreeGroup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ShreeGroup.Controllers
{
    public class AccountController : Controller
    {
        AccountRepository repository = null;

        public AccountController()
        {
            repository = new AccountRepository();
        }

        // GET: Account
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserModel userModel)
        {
            if(ModelState.IsValid)
            {
                bool isValid=repository.LoginUser(userModel);
                if(isValid)
                {
                    FormsAuthentication.SetAuthCookie(userModel.UserName, false);
                    return RedirectToAction("Index", "RegisterUser");
                }
                ModelState.AddModelError("", "Invalid username and password");
            }
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }

        //[HttpPost]
        //public ActionResult SignUp(UserModel userModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string result =  repository.AddUser(userModel);
        //        return RedirectToAction("Login");
        //    }
        //    return View();
        //}

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}