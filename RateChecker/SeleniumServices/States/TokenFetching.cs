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

        var devTools = (driver as RemoteWebDriver).GetDevToolsSession();

        
        await Task.Delay(2000);

        try
        {
            var network = driver.Manage().Network;
            network.StartMonitoring().Wait();
            

            var searchReq = new TaskCompletionSource<(string token, string cookie)>();
            var requestsProccessed = new TaskCompletionSource<bool>();
            int counter = 0;


            void callback(object obj, NetworkRequestSentEventArgs req)
            {
                Interlocked.Increment(ref counter);
                if (req.RequestUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
                {
                    var token = req.RequestHeaders["csrftoken"];
                    var cookie = req.RequestHeaders["Cookie"];

                    network.ClearRequestHandlers();
                    //network.StopMonitoring().Wait();
                    
                    network.NetworkRequestSent -= callback;
                    searchReq.TrySetResult((token, cookie));                    
                }
            };

            network.NetworkRequestSent += callback;
            network.NetworkResponseReceived += (sender, e) =>
            {
                Interlocked.Decrement(ref counter);


                if (searchReq.Task.IsCompleted && counter == 0)
                {
                    requestsProccessed.TrySetResult(true);
                    network.StopMonitoring().Wait();
                }
            };
            


            driver.Navigate().GoToUrl("https://p2p.binance.com/ru/trade/all-payments/USDT?fiat=RUB");

            Task.Delay(10000).Wait();
            searchReq.Task.Wait();
            requestsProccessed.Task.Wait();

            var result = searchReq.Task.Result;

            context.Token = result.token;
            context.Cookie = result.cookie;

            return TriggerEnum.Success;
        }
        catch (Exception)
        {
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