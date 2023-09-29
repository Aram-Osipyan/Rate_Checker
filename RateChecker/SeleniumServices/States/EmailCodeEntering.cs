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

        var emailButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".bn-mfa-overview-step-wrapper .bn-mfa-overview-step:nth-child(2)")));
        var now = DateTime.Now;
        

        /// IMAP
        string messageHtml = "";
        using (var clientp = new ImapClient())
        {
            await clientp.ConnectAsync("imap.gmail.com", 993, true);
            await clientp.AuthenticateAsync("binancenoviy@gmail.com", "ttxo bfhg yzlu kbxm");
            
            var inbox = clientp.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            var ts = new TaskCompletionSource<bool>();
            await Console.Out.WriteLineAsync($"message count: {inbox.Count}");

            var done = new CancellationTokenSource(new TimeSpan(0, 0, 30));

            inbox.CountChanged += (obj, args) =>
            {
                ts.SetResult(true);
            };

            emailButton.Click();
            await clientp.IdleAsync(done.Token);

                       
            await ts.Task;

            var query = SearchQuery.FromContains("Binance")
                .And(SearchQuery.SubjectContains("Подтверждение входа"))
                .And(SearchQuery.DeliveredAfter(now));

            Console.WriteLine("Total messages: {0}", inbox.Count);
            Console.WriteLine("Recent messages: {0}", inbox.Recent);

            foreach (var uid in (await inbox.SearchAsync(query)).OrderByDescending(x => x))
            {
                var message = await inbox.GetMessageAsync(uid);
                Console.WriteLine("[match] {0}: {1}", uid, message.Subject);
                //Console.WriteLine("[body] {0}: {1}", uid, message.HtmlBody);
                messageHtml = message.HtmlBody;

                inbox.AddFlags(uid, MessageFlags.Seen, true);
                break;
            }

            await clientp.DisconnectAsync(true);
        }
        /// /IMAP

        string pattern = @"<b>\s*(\d\d\d\d\d\d)\s*<\/b>";
        var emailCode = Regex.Match(messageHtml, pattern).Groups.Values.Skip(1).First().Value;

        var emailCodeInput = wait.Until(d => d.FindElement(By.CssSelector("input.bn-textField-input")));
        emailCodeInput.SendKeys(emailCode);

        // TODO: wait for page loading
        //await Task.Delay(10000);

        return TriggerEnum.Success;
    }
}
