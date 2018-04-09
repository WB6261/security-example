using System.Threading.Tasks;
using Fisher.Bookstore.Api.Data;
using Fisher.Bookstore.Api.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Fisher.Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly BookstoreContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, BookstoreContext db)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);
            if (result.Succeeded)
            {
                return Ok("Logged In");
            }
            if (result.IsLockedOut)
            {
                return BadRequest("Account Locked");
            }
            if (result.IsNotAllowed)
            {
                return Unauthorized();
            }

            return BadRequest("Unknown Error");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Login login)
        {
            var user = new ApplicationUser()
            {
                UserName = login.Email,
                Email = login.Email
            };

            var result = await userManager.CreateAsync(user, login.Password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return Ok("Registered");
            }
            return BadRequest(result.Errors);
        }
    }
}