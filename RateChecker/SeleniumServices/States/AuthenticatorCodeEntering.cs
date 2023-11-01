using Docker.DotNet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OtpNet;
using RateChecker.Common;
using RateChecker.StateMachine;
using SeleniumExtras.WaitHelpers;

namespace RateChecker.SeleniumServices.States;
public class AuthenticatorCodeEntering : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public AuthenticatorCodeEntering(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        try
        {
            var driver = context.Driver;

            driver.SwitchTo().Window(driver.CurrentWindowHandle);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));


            var authenticatorButton = wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:first-child"))).FirstOrDefault();
            //var authenticatorButton = wait.Until(d => d.FindElement(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:first-child")));
            authenticatorButton.Click();



            //await Task.Delay(1000);
            await Task.Yield();

            var authenticatorInput = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input.bn-textField-input")));
            //var authenticatorInput = wait.Until(d => d.FindElement(By.CssSelector("input.bn-textField-input")));

            //var secretKey = "e7JT+GoUeGvk6g==";
            var secretKey = context.Input.AuthenticatorKey;

            var bytes = Convert.FromBase64String(secretKey);
            var totp = new Totp(bytes);
            var totpCode = totp.ComputeTotp();

            authenticatorInput.SendKeys(totpCode);

            return TriggerEnum.Success;
        }
        catch (Exception ex)
        {
            await Console.Out.WriteLineAsync(ex.Message);

            return TriggerEnum.SkipAuthenticatorStep;
        }
    }
}
