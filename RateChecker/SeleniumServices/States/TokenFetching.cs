using Docker.DotNet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V115;
using OpenQA.Selenium.DevTools.V115.Fetch;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Org.BouncyCastle.Ocsp;
using RateChecker.Common;
using RateChecker.StateMachine;
using RestSharp;
using SeleniumExtras.WaitHelpers;
using System.Net;
using System.Text;
using System.Text.Json;
using DevToolsSessionDomains = OpenQA.Selenium.DevTools.V115.DevToolsSessionDomains;
using Network = OpenQA.Selenium.DevTools.Network;

namespace RateChecker.SeleniumServices.States;
public class TokenFetching : State<StateMachineContext, TriggerEnum, StateEnum>
{
    private static object _locker = new object();
    public TokenFetching(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {

        var driver = context.Driver;

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        var yesBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[text()=\"Yes\"]")));

        yesBtn.Click();

        var devtools = (driver as RemoteWebDriver);
        var session = devtools.GetDevToolsSession();

        var domains = session.GetVersionSpecificDomains<DevToolsSessionDomains>();

        await domains.Network.Enable(new OpenQA.Selenium.DevTools.V115.Network.EnableCommandSettings());
        await Task.Delay(2000);

        //var network = driver.Manage().Network;
        //await (driver as IDevTools).GetDevToolsSession().Domains.Network.DisableFetch();


        var ts = new TaskCompletionSource<(string token, string cookie)>();

        /*
        void callback(object obj, NetworkRequestSentEventArgs req)
        {
            lock (_locker)
            {
                if (req.RequestUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
                {
                    var token = req.RequestHeaders["csrftoken"];
                    var cookie = req.RequestHeaders["Cookie"];

                    //network.ClearRequestHandlers();
                    //network.StopMonitoring().Wait();
                    (driver as IDevTools).CloseDevToolsSession();

                    ts.TrySetResult((token, cookie));
                    //network.NetworkRequestSent -= callback;
                }
            }
        };*/
        domains.Network.RequestWillBeSent += (sender, e) =>
        {
            if (e.Request.Url == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
            {
                var token = e.Request.Headers["csrftoken"];
                
                var cookie = GetCookiesAsString(driver);

                //network.ClearRequestHandlers();
                //network.StopMonitoring().Wait();
                //(driver as IDevTools).CloseDevToolsSession();

                ts.TrySetResult((token, cookie));
                //network.NetworkRequestSent -= callback;
            }
        };
        //network.NetworkRequestSent += callback;
        //network.StartMonitoring().Wait();
        //network as V115Network





        driver.Navigate().GoToUrl("https://p2p.binance.com/ru/trade/all-payments/USDT?fiat=RUB");
        //driver.Navigate().Refresh();

        var result = await ts.Task;
        string sessionString = (driver as RemoteWebDriver).SessionId.ToString();

        var client = new RestClient("http://selenoid-ui:8080");
        var request = new RestRequest($"wd/hub/session/{sessionString}", Method.Delete);
            
        var response = await client.ExecuteAsync(request);

            


        context.Token = result.token;
        context.Cookie = result.cookie;

        return TriggerEnum.Success;

    }

    public string GetCookiesAsString(IWebDriver driver)
    {
        var cookies = driver.Manage().Cookies.AllCookies;

        var builder = new StringBuilder();
        foreach (var cookie in cookies)
        {
            builder.Append($"{cookie.Name}={cookie.Value}; ");
        }


        return builder.ToString();
    }
}