using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RateChecker.SeleniumServices;

namespace RateChecker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IWebDriverLogin _webDriverLogin;

        public ValuesController(IWebDriverLogin webDriverLogin)
        {
            _webDriverLogin = webDriverLogin;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var result = await _webDriverLogin.Login();

            return new JsonResult(new { Token = result.token, Cookie = result.cookie });
        }
    }
}
