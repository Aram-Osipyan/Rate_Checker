using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;
using RestSharp;
using System.Net.Mail;
using System.Text.RegularExpressions;
using MailKit.Net.Imap;
using MailKit;
using MailKit.Search;
using OtpNet;
using RateChecker.StateMachine;
using RateChecker.Common;
using RateChecker.Domain;

namespace RateChecker.SeleniumServices;
public class WebDriverLogin : IWebDriverLogin
{
    private readonly IStateMachineFactory factory;

    public WebDriverLogin(IStateMachineFactory factory)
    {
        this.factory = factory;
    }
    public async Task<(string token, string cookie)> Login(TokenRefreshInput input)
    {
        var context = new StateMachineContext();
        var stateMachine = factory.GetStateMachine();
        context.Input = input;
        await stateMachine.Run(StateEnum.DriverInitialization, context);

        return (context.Token, context.Cookie);
    }
}
