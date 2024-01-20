using Docker.DotNet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.DevTools.V115.Fetch;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using RateChecker.Common;
using RateChecker.StateMachine;
using RestSharp;
using SeleniumExtras.WaitHelpers;

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

       

        
        await Task.Delay(2000);

        try
        {
            var network = driver.Manage().Network;



            var ts = new TaskCompletionSource<(string token, string cookie)>();

            void callback(object obj, NetworkRequestSentEventArgs req)
            {
                if (req.RequestUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
                {
                    var token = req.RequestHeaders["csrftoken"];
                    var cookie = req.RequestHeaders["Cookie"];

                    network.ClearRequestHandlers();
                    //network.StopMonitoring().Wait();

                    network.NetworkRequestSent -= callback;
                    ts.TrySetResult((token, cookie));                    
                }
            };

            network.NetworkRequestSent += callback;
            network.NetworkResponseReceived += (sender, e) =>
            {
                if (e.ResponseUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
                {
                    network.StopMonitoring().Wait();
                }
            };
            


            driver.Navigate().GoToUrl("https://p2p.binance.com/ru/trade/all-payments/USDT?fiat=RUB");
            driver.Navigate().Refresh();



            // confirm "input[name=\"password\"]"
            var checkbox = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("label.css-o78533")));
            checkbox.Click();
            var confirmButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button.css-gyhchg")));
            confirmButton.Click();

            // choose rub option
            var listElem = wait.Until((ExpectedConditions.ElementIsVisible(By.CssSelector("div.css-11ho4jc"))));

            listElem.Click();

            var rubOption = wait.Until(d => d.FindElement(By.CssSelector("li#RUB")));
            await network.StartMonitoring();


            rubOption.Click();

            ts.Task.Wait();

            var result = ts.Task.Result;

            context.Token = result.token;
            context.Cookie = result.cookie;

            return TriggerEnum.Success;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);
            return TriggerEnum.Failure;
        }
    }

    private async Task RemoveDriverSession(string session)
    {
        var client = new RestClient("http://selenoid-ui:8080");
        var request = new RestRequest($"wd/hub/session/{session}", Method.Delete);

        var response = await client.ExecuteAsync(request);
    } 
}