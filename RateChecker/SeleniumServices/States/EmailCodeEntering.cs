using Docker.DotNet.Models;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using OpenQA.Selenium;
using RateChecker.Common;
using RateChecker.StateMachine;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Security.Policy;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Threading.Tasks;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace RateChecker.SeleniumServices.States;
public class EmailCodeEntering : State<StateMachineContext, TriggerEnum, StateEnum>
{
    public EmailCodeEntering(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        var driver = context.Driver;

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        var now = DateTime.Now;
        try
        {
            var emailButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:nth-child(2)")));
            emailButton.Click();
        }
        catch (Exception e)
        {
            await Console.Out.WriteLineAsync(e.Message);
        }      


        /// IMAP
        string messageHtml = "";
        using var clientp = context.ImapClient;
        var inbox = context.Inbox;

        if (context.IdleTask.Status == TaskStatus.Running || context.IdleTask.Status == TaskStatus.WaitingForActivation)
        {
            context.IdleTask.Wait();
        }

        if (context.ImapMessageWaiter.Task.Status == System.Threading.Tasks.TaskStatus.Running)
        {
            await context.ImapMessageWaiter.Task.WaitAsync(TimeSpan.FromSeconds(30));
        }

        var query = SearchQuery.FromContains("Binance")
            //.And(SearchQuery.Or(SearchQuery.SubjectContains("Подтверждение входа"), SearchQuery.SubjectContains("Login Verification")))
            .And(SearchQuery.DeliveredAfter(now));

        Console.WriteLine("Total messages: {0}", inbox.Count);

        foreach (var uid in (await inbox.SearchAsync(query)).OrderByDescending(x => x))
        {
            var message = await inbox.GetMessageAsync(uid);
            Console.WriteLine("[match] {0}: {1}", uid, message.Subject);
            //Console.WriteLine("[body] {0}: {1}", uid, message.HtmlBody);
            messageHtml = message.HtmlBody;

            inbox.AddFlags(uid, MessageFlags.Seen, true);
            break;
        }

        if (clientp is not null)
        {
            await clientp?.DisconnectAsync(true);
        }        
        
        /// /IMAP
        

        string pattern = @"<strong>\s*(\d\d\d\d\d\d)\s*<\/strong>";
        var emailCode = Regex.Match(messageHtml, pattern).Groups.Values.Skip(1).First().Value;

        var emailCodeInput = wait.Until(d => d.FindElement(By.CssSelector("input.bn-textField-input")));
        emailCodeInput.SendKeys(emailCode);

        // TODO: wait for page loading
        await Task.Delay(2000);

        return TriggerEnum.Success;
    }
}
