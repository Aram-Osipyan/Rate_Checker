using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using RateChecker.Common;
using RateChecker.StateMachine;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.DevTools;
using FluentMigrator.Runner.Versioning;
using System.Reflection;

namespace RateChecker.SeleniumServices.States;
internal static class ReflectionExtensions
{
    private static IEnumerable<Type> BaseTypes(this Type type)
    {
        while (type.BaseType != null)
        {
            yield return type.BaseType;
            type = type.BaseType;
        }
    }

    internal static FieldInfo GetFieldInfo(this object value, string memberName, BindingFlags? bindingFlags = null)
    {
        bindingFlags ??= BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        var type = value.GetType();
        return new[] { type }.Concat(type.BaseTypes()).Select(t => t.GetField(memberName, bindingFlags.Value)).First(
            f => f != null);
    }

    internal static TValue GetFieldValue<TValue>(this object instance, string fieldName) =>
        instance == null
            ? throw new ArgumentNullException(nameof(instance))
            : (TValue)(instance.GetFieldInfo(fieldName)).GetValue(instance);

    internal static void SetFieldValue<TValue>(this object instance, string fieldName, TValue value) =>
        (instance.GetFieldInfo(fieldName)).SetValue(instance, value);
}
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
            ["sessionTimeout"] = "4m",

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


        var caps = new RemoteSessionSettings();
        caps.AddFirstMatchDriverOption(options);

        var dict = caps.ToDictionary();
        // IWebDriver driver = await Task.Run(() => new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), options.ToCapabilities(), TimeSpan.FromMinutes(2)));
        //IWebDriver driver = new ChromeDriver(options);
        //var driver = new RemoteWebDriver(new Uri("http://selenoid:4444/wd/hub"), options.ToCapabilities(), TimeSpan.FromMinutes(4));

        var driver = new RemoteWebDriver(new Uri("http://selenoid:4444/wd/hub"), caps, TimeSpan.FromMinutes(4));


        var capabilitiesDictionary = (driver as WebDriver)?.Capabilities.GetFieldValue<Dictionary<string, object>>("capabilities");
        //capabilitiesDictionary["se:cdp"] = $"ws://{host}/devtools/{sessionId}/page";
        capabilitiesDictionary["se:cdpVersion"] = "115.0";

        await Task.Run(() => driver.Navigate().GoToUrl("https://accounts.binance.com/en/login?"));

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));

        var cookieButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button#onetrust-accept-btn-handler")));

        cookieButton.Submit();

        context.Driver = driver;

        return TriggerEnum.Success;
    }
}
