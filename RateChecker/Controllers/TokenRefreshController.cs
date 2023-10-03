using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RateChecker.Domain;
using RateChecker.SeleniumServices;

namespace RateChecker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenRefreshController : ControllerBase
    {
        private readonly IWebDriverLogin _webDriverLogin;

        public TokenRefreshController(IWebDriverLogin webDriverLogin)
        {
            _webDriverLogin = webDriverLogin;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TokenRefreshInput input)
        {
            var result = await _webDriverLogin.Login(input);

            return new JsonResult(new { Token = result.token, Cookie = result.cookie });
        }
    }
}
