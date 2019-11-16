using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        // ---------------------------------------------------------------------
        // Index
        // ---------------------------------------------------------------------
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("index");
        }
        // ---------------------------------------------------------------------
        // Registrating to db
        // ---------------------------------------------------------------------
        [HttpPost("/Register")]
        public IActionResult Register(User user)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email","Email already in use!");
                    return View("Index");
                }
                else if(dbContext.Users.Any(u => u.FirstName == user.FirstName))
                {
                    ModelState.AddModelError("FirstName","First Name is already in use!");
                    return View("Index");
                }
                else if(dbContext.Users.Any(u => u.LastName == user.LastName))
                {
                    ModelState.AddModelError("LastName","Last Name is already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                user.Password = Hasher.HashPassword(user,user.Password);
                dbContext.Add(user);
                dbContext.SaveChanges();
                int user_id = user.UserId;
                HttpContext.Session.SetInt32("user_id",user_id);
                return RedirectToAction("Dashbord");
            }
            return View("Index");
        }
        // ---------------------------------------------------------------------
        // Login Page
        // ---------------------------------------------------------------------
            [HttpGet("Login")]
            public IActionResult Login()
            {
                return View("Login");
            }
        // ---------------------------------------------------------------------
        // Login TO success or BELT EXAM would be this page !
        // ---------------------------------------------------------------------
        [HttpPost("logToSuccess")]
        public IActionResult LoginSuccess(LoginUser userSubmision)
        {
            if(ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmision.Email);
                if(userInDb == null)
                {
                    ModelState.AddModelError("Email","Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmision,userInDb.Password,userSubmision.Password);
                if(result == 0)
                {
                    ModelState.AddModelError("Password","Invalid Email/Password");
                    return View("Login");
                }
                //--------------------- Creating session after all those checks -------------------
                int user_id = userInDb.UserId;
                HttpContext.Session.SetInt32("user_id",user_id);
                return RedirectToAction("Dashbord");
            }
            return View("Login");
        }

        // ---------------------------------------------------------------------
        // CLEAR SESSION
        // ---------------------------------------------------------------------
            [HttpGet("/clear")]
            public IActionResult clearSession()
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }
        // ---------------------------------------------------------------------
        // Success PAGE potential belt-exam page
        // ---------------------------------------------------------------------
        // ---------------------------------------------------------------------
        // Wedding Dashbord
        // ---------------------------------------------------------------------

            [HttpGet("/Dashbord")]
            public IActionResult Dashbord()
            {
                if(HttpContext.Session.GetInt32("user_id") == null)
                {
                    return RedirectToAction("Login");
                }
                //*** Get The user
                User oneUser = dbContext.Users.Include(r => r.Rsvps).ThenInclude(w=>w.Wedding).FirstOrDefault(u =>u.UserId == (int)HttpContext.Session.GetInt32("user_id"));

                //*** Get all the weddings
                List <Wedding> all_weddings = dbContext.Weddings.Include(r =>r.Rsvps).ThenInclude(w => w.User).ToList();



                ViewBag.all_weddings = all_weddings;
                ViewBag.oneUser = oneUser;

                    return View("Dashbord");
            }
        // ---------------------------------------------------------------------
        // Adding Wedding input VIEW PAGE
        // ---------------------------------------------------------------------
        [HttpGet("/addWedding")]
        public IActionResult AddWedding()
        {
            if(HttpContext.Session.GetInt32("user_id")== null)
            {
                return RedirectToAction("Login");
            }
            @ViewBag.user_id = (int)HttpContext.Session.GetInt32("user_id");
            return View("AddWedding");
        }
        // ---------------------------------------------------------------------
        // POST FORM CREATE WEDDING
        // ---------------------------------------------------------------------
        [HttpPost("/createWedding")]
        public IActionResult CreateWedding(Wedding newwed)
        {
            if(HttpContext.Session.GetInt32("user_id") == null)
            {
                return RedirectToAction("Login");
            }

            if(ModelState.IsValid)
            {
            if(newwed.Date < DateTime.Now)
            {
                ModelState.AddModelError("Date","Wedding must be in the future");

                //*** i need to assign this session again because i need it when i View it back
                @ViewBag.user_id = (int)HttpContext.Session.GetInt32("user_id");
                return View("AddWedding");
            }
            //*** The same here i need to assign session again !
            newwed.UserId = (int)HttpContext.Session.GetInt32("user_id");
            dbContext.Add(newwed);
            dbContext.SaveChanges();
            int wed_id = newwed.WeddingId;
            return Redirect($"WeddingINFO/{wed_id}");
            }
            else
            {
                @ViewBag.user_id = (int)HttpContext.Session.GetInt32("user_id");
                ViewBag.errors = ModelState.Values;
                return View("AddWedding");
            }
        }

        // ---------------------------------------------------------------------
        // VIEW wedINFORMATION
        // ---------------------------------------------------------------------
        [HttpGet("/WeddingINFO/{wed_id}")]
        public IActionResult WeddingINFO(int wed_id)
        {
            if(HttpContext.Session.GetInt32("user_id")== null)
            {
                ModelState.AddModelError("Email","Don't be sneaky");
                ModelState.AddModelError("Password","I know what you did last summer :) !!!");
                return View("Login");
            }
            Wedding onewedding = dbContext.Weddings.Include(r => r.Rsvps).ThenInclude(u=>u.User).FirstOrDefault(w => w.WeddingId == wed_id);
            ViewBag.onewed = onewedding;

            return View("WeddingINFO");
        }
        // ---------------------------------------------------------------------
        // DELETE WEDDING
        // ---------------------------------------------------------------------

        [HttpGet("/delete/{WeddingId}")]
        public IActionResult DeleteWedding(int WeddingId)
        {
            if(HttpContext.Session.GetInt32("user_id")==null){
                return RedirectToAction("Login");
            }
            else
            {
                Wedding wedding = dbContext.Weddings.FirstOrDefault(w =>w.WeddingId == WeddingId);
                dbContext.Remove(wedding);
                dbContext.SaveChanges();
            return RedirectToAction("Dashbord");
            }
        }
        // ---------------------------------------------------------------------
        // Join Wedding
        // ---------------------------------------------------------------------
        [HttpGet("/join/{WeddingId}")]
        public IActionResult JointWedding(int WeddingId)
        {
            if(HttpContext.Session.GetInt32("user_id") == null)
            {
                return RedirectToAction("Login");
            }
            //*** Retrive the user with all his tables
            User user = dbContext.Users.Include(u=>u.Rsvps)
                .ThenInclude(r=>r.Wedding)
                    .FirstOrDefault(u=>u.UserId == (int)HttpContext.Session.GetInt32("user_id"));

            //*** retrive the wedding with every table
            Wedding wedding = dbContext.Weddings.Include(w=>w.Rsvps)
                .ThenInclude(r=>r.User).FirstOrDefault(w=>w.WeddingId == WeddingId);
            
            //***Build RSVP and then import it in  the DB :)
            Rsvp newRVSP = new Rsvp
            {
                UserId = user.UserId,
                User = user,
                WeddingId = wedding.WeddingId,
                Wedding = wedding,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            
            //*** Now we have the newRsvp and is time to put it in DB
            dbContext.Add(newRVSP);
            dbContext.SaveChanges();
            return RedirectToAction("Dashbord");
        }
        
        // ---------------------------------------------------------------------
        // Exit Wedding (un-RSVP)
        // ---------------------------------------------------------------------
        [HttpGet("exitWedding/{WeddingId}")]
        public IActionResult exitWedding(int WeddingId)
        {
            if(HttpContext.Session.GetInt32("user_id") == null)
            {
                return RedirectToAction("Login");
            }

            //*** Find user_id and the WeddingId from the Rout and quiry the quiry
            int user_id = (int)HttpContext.Session.GetInt32("user_id");

            //*** Find the Quiery where user_id and WeddingId are matching and update it
            Rsvp rsvp = dbContext.Rsvps
                    .FirstOrDefault(u => u.UserId == user_id && u.WeddingId == WeddingId);

            dbContext.Remove(rsvp);
            dbContext.SaveChanges();
            return RedirectToAction("Dashbord");
        }



























        // ---------------------------------------------------------------------
        // ERRORRR
        // ---------------------------------------------------------------------

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
