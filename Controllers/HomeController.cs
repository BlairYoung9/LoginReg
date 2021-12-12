using Microsoft.EntityFrameworkCore;
using LoginReg.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
// Other using statements
namespace LoginReg.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
     
        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
     
        [HttpGet("")]
        public IActionResult Index()
        {            
            return View();
        }
        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            // Check initial ModelState
            if(ModelState.IsValid)
            {
                // If a User exists with provided email
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    // Manually add a ModelState error to the Email field, with provided
                    // error message
                    ModelState.AddModelError("Email", "Email already in use!");
                    
                    return View("index");
                    // You may consider returning to the View at this point
                }
                else
                {
                    
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    user.Password = Hasher.HashPassword(user, user.Password);
                    //Save your user object to the database
                    dbContext.Add(user);
                    dbContext.SaveChanges();

                    return RedirectToAction("success");
                    //return View("success"); didnt work
                }
            }
            else
            {
                return View("index");
            }
            // other code
        }
        [HttpGet("success")]
        public IActionResult success()
        {
            return View();
        }
        [HttpGet("login")]
        public IActionResult login()
        {
            return View();
        }
        [HttpPost]
        [Route("user")]
        public IActionResult Login(LoginUser userSubmission)
        {
            if(ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                // If no user exists with provided email
                if(userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return RedirectToAction("login");
                }
                
                // Initialize hasher object
                var hasher = new PasswordHasher<LoginUser>();
                
                // verify provided password against hash stored in db
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                
                // result can be compared to 0 for failure
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return RedirectToAction("login");
                    // handle failure (this should be similar to how "existing email" is handled)
                }
                else
                {
                    return RedirectToAction("success");
                }
            }
            else
            {
                return RedirectToAction("login");
            }
        }
    }
}