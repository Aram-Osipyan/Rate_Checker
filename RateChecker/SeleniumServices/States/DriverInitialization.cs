using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using RateChecker.Common;
using RateChecker.StateMachine;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace RateChecker.SeleniumServices.States;
public class DriverInitialization : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public DriverInitialization(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        var options = new ChromeOptions();
        //options.BrowserVersion = "114.0";
        var userAgent = "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25";
        options.AddAdditionalOption("selenoid:options", new Dictionary<string, object>
        {
            /* How to add test badge */
            ["name"] = $"{DateTime.Now}",

            /* How to set session timeout */
            ["sessionTimeout"] = "15m",

            /* How to set timezone */
            ["env"] = new List<string>() {
                "TZ=UTC"
            },

            /* How to add "trash" button */
            ["labels"] = new Dictionary<string, object>
            {
                ["manual"] = "true"
            },
            ["enableVNC"] = true,
        });

        options.AddArguments(new List<string>() { "no-sandbox", "disable-gpu" });
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument($"--user-agent={userAgent}");
        options.AddAdditionalChromeOption("useAutomationExtension", false);

        // IWebDriver driver = await Task.Run(() => new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), options.ToCapabilities(), TimeSpan.FromMinutes(2)));
        IWebDriver driver = new ChromeDriver(options);

        await Task.Run(() => driver.Navigate().GoToUrl("https://accounts.binance.com/en/login?"));

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var cookieButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button#onetrust-accept-btn-handler")));

        cookieButton.Submit();

        context.Driver = driver;

        return TriggerEnum.Success;
    }
}
