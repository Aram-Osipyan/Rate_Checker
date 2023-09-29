using Docker.DotNet.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using RateChecker.Common;
using RateChecker.StateMachine;

namespace RateChecker.SeleniumServices.States;
public class PasswordEntering : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public PasswordEntering(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        var driver = context.Driver;
        driver.SwitchTo().Window(driver.CurrentWindowHandle);

        await Task.Yield();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        var passwordInput = wait.Until(d =>
        {
            return d.FindElement(By.CssSelector("input[name=\"password\"]"));
        });
        var password = "28500Rama56";
        passwordInput.SendKeys(password);

        var loginSubmitBtn = driver.FindElement(By.CssSelector("button#click_login_submit"));
        loginSubmitBtn.Click();

        return TriggerEnum.Success;
    }
}
