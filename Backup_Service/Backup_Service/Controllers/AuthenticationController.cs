using Microsoft.AspNetCore.Mvc;
using Backup_Service.Models;
using Backup_Service.Services;
using Backup_Service.Data.DataModels;
using System.Threading.Tasks;
using System.IO;

namespace Backup_Service.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly ILoginService _loginService;

        public AuthenticationController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task<IActionResult> Login()
        {
            string token = null;
            if (!Request.Cookies.TryGetValue("token", out token))
                return View("Login");

            var user = await _loginService.CheckToken(token);
            if (user == null)
                return View("Login");

            return RedirectToAction("Index", "Home");;
        }

        [Route("register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> CheckLogin(LoginFormModel model)
        {
            var user = await _loginService.CheckLogin(model.Login, model.Password);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            Response.Cookies.Append("token", _loginService.CreateToken(user.Id, user.Password));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Backups"));
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> CheckRegister(User user)
        {
            var authResult = await _loginService.Register(
                    user.Username,
                    user.Password);

            if (authResult is null)
            {
                return RedirectToAction ("Register");
            }

            await _loginService.Register(user.Username, user.Password);

            return RedirectToAction("Index", "Home");
        }
    }
}
