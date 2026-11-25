using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web1.Data;
using Web1.Models;
using Web1.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Web1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check whether username already exists
            var existingUserName = await _context.Users
                .AnyAsync(u => u.UserName == model.UserName);

            if (existingUserName)
            {
                ModelState.AddModelError(
                    "UserName",
                    "Username already taken."
                );
            }

            // Check whether email already exists
            var existingEmail = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (existingEmail)
            {
                ModelState.AddModelError(
                    "Email",
                    "Email already registered."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email
            };

            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                model.Password
            );

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    "",
                    "Username or email is already registered. Please try different details."
                );

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Registration successful! Please log in.";

            return RedirectToAction("Login");
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                model.Password
            );

            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
{
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
};

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Dashboard");
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction("Index", "Home");
        }

        //----------------------------------------------------------------------------------------------------------

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
