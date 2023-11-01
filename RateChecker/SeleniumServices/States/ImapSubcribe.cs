using MailKit.Net.Imap;
using MailKit;
using RateChecker.Common;
using RateChecker.StateMachine;

namespace RateChecker.SeleniumServices.States;
public class ImapSubcribe : State<StateMachineContext, TriggerEnum, StateEnum>
{
    private Dictionary<string, string> _imapServers = new Dictionary<string, string>()
    {
        { "gmail", "imap.gmail.com" },
        { "mail", "imap.mail.ru" },
    };
    public ImapSubcribe(StateEnum stateEnum) : base(stateEnum)
    {
    }

    public override async Task<TriggerEnum> Perform(StateMachineContext context)
    {
        var clientp = new ImapClient();

        var imapServer = _imapServers.FirstOrDefault(x => context.Input.ImapEmail.Contains(x.Key)).Value;

        await clientp.ConnectAsync(imapServer, 993, true);
        //await clientp.AuthenticateAsync("binancenoviy@gmail.com", "ttxo bfhg yzlu kbxm");
        await clientp.AuthenticateAsync(context.Input.ImapEmail, context.Input.ImapPassword);

        var inbox = context.Inbox = clientp.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);

        var ts = context.ImapMessageWaiter = new TaskCompletionSource<bool>();
        await Console.Out.WriteLineAsync($"message count: {inbox.Count}");

        var done = new CancellationTokenSource(new TimeSpan(0, 1, 30));

        inbox.CountChanged += (obj, args) =>
        {
            Console.WriteLine("message count changed");
            ts.SetResult(true);
        };


        context.IdleTask = clientp.IdleAsync(done.Token);

        return TriggerEnum.Success;
    }
}
