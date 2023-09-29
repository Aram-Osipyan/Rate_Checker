using Docker.DotNet.Models;
using OpenQA.Selenium;
using RateChecker.Common;
using RateChecker.StateMachine;

namespace RateChecker.SeleniumServices.States;
public class UsernameEntering : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public UsernameEntering(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        var driver = context.Driver;
        var emailInput = driver.FindElement(By.CssSelector("input#username"));
        var email = "binancenoviy@gmail.com";


        emailInput.SendKeys(email);

        var nextButton = driver.FindElement(By.CssSelector("button#click_login_submit"));

        nextButton.Click();


        await Task.Delay(3000);

        return TriggerEnum.Success;
    }
}
