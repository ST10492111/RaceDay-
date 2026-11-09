using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync("Auth/login", model);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);

                // Extract the token and user info from the API response
                var token = jsonDoc.RootElement.GetProperty("token").GetString();
                var user = jsonDoc.RootElement.GetProperty("user");
                var role = user.GetProperty("role").GetString();
                var firstName = user.GetProperty("firstName").GetString();

                // Save to Session
                HttpContext.Session.SetString("JwtToken", token ?? "");
                HttpContext.Session.SetString("UserRole", role ?? "");
                HttpContext.Session.SetString("UserName", firstName ?? "");

                // Redirect based on role
                if (role == "Organiser")
                    return RedirectToAction("Dashboard", "Organiser");
                else
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _apiService.PostAsync("Auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                // Automatically log them in after registering
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Registration failed. Email might already exist.");
            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
