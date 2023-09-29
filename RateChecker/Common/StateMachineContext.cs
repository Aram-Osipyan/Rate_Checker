using OpenQA.Selenium;

namespace RateChecker.Common;
public class StateMachineContext
{
    public IWebDriver Driver { get; set; }
    public DateTime StartTime { get; set; }
    public string Cookie { get; set; }
    public string Token { get; set; }
}
