using OpenQA.Selenium;
using RateChecker.Domain;

namespace RateChecker.Common;
public class StateMachineContext
{
    public IWebDriver Driver { get; set; }
    public DateTime StartTime { get; set; }
    public string Cookie { get; set; }
    public string Token { get; set; }
    public TokenRefreshInput Input { get; set; }
}
