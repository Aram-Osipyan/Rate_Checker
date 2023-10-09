using Docker.DotNet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RateChecker.Common;
using RateChecker.StateMachine;
using SeleniumExtras.WaitHelpers;

namespace RateChecker.SeleniumServices.States;
public class TokenFetching : State<StateMachineContext, TriggerEnum, StateEnum>
{
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
            driver.Navigate().GoToUrl("https://p2p.binance.com/ru/trade/all-payments/USDT?fiat=RUB");

            var network = driver.Manage().Network;
            network.StartMonitoring().Wait();
            
            var ts = new TaskCompletionSource<(string token, string cookie)>();


            void callback(object obj, NetworkRequestSentEventArgs req)
            {
                if (req.RequestUrl == "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search")
                {
                    var token = req.RequestHeaders["csrftoken"];
                    var cookie = req.RequestHeaders["Cookie"];

                    ts.TrySetResult((token, cookie));
                }
            };

            network.NetworkRequestSent += callback;

            //driver.Navigate().Refresh();

            var result = await ts.Task;

            network.StopMonitoring().Wait();

            context.Token = result.token;
            context.Cookie = result.cookie;

            return TriggerEnum.Success;
        }
        catch (Exception)
        {
            return TriggerEnum.Failure;
        }
    }
}