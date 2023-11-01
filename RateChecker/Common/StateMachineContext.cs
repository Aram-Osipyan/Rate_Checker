using MailKit;
using MailKit.Net.Imap;
using OpenQA.Selenium;
using RateChecker.Domain;

namespace RateChecker.Common;
public class StateMachineContext
{
    public IWebDriver Driver { get; set; }
    public DateTime StartTime { get; set; }
    public string Cookie { get; set; }
    public string Token { get; set; }
    public TokenRefreshInput Input { get; set; }
    public TaskCompletionSource<bool> ImapMessageWaiter { get; set; }
    public ImapClient ImapClient { get; set; }
    public IMailFolder Inbox { get; set; }
    public Task IdleTask { get; set; }
}
