using System.Threading.Tasks;
using Fisher.Bookstore.Api.Data;
using Fisher.Bookstore.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace Fisher.Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly BookstoreContext db;

        public AccountsController(BookstoreContext db)
        {
            this.db = db;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var user = new ApplicationUser();

            user.Email = login.Email;
            user.UserName = "Login";

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Login login)
        {
            var user = new ApplicationUser();

            user.Email = login.Email;
            user.UserName = "Register";

            return Ok(user);
        }
    }
}