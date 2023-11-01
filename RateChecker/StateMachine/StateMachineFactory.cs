using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium.DevTools.V113.DOM;
using RateChecker.Common;
using RateChecker.SeleniumServices.States;

namespace RateChecker.StateMachine;
public class StateMachineFactory : IStateMachineFactory
{
    private readonly IServiceProvider _serviceProvider;

    public StateMachineFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private StateMachine<TriggerEnum, StateMachineContext, StateEnum> _stateMachine;
    public StateMachine<TriggerEnum, StateMachineContext, StateEnum> GetStateMachine()
    {
        if (_stateMachine != null)
        {
            return _stateMachine;
        }

        _stateMachine = new();
        ConfigureStateMachine();

        return _stateMachine;
    }

    private void ConfigureStateMachine()
    {
        var driverInitialization = _serviceProvider.GetService<DriverInitialization>();
        var usernameEntering = _serviceProvider.GetService<UsernameEntering>();
        var passwordEntering = _serviceProvider.GetService<PasswordEntering>();
        var captureSolving = _serviceProvider.GetService<CaptureSolving>();
        var authenticatorCodeEntering = _serviceProvider.GetService<AuthenticatorCodeEntering>();
        var emailCodeEntering = _serviceProvider.GetService<EmailCodeEntering>();
        var tokenFetching = _serviceProvider.GetService<TokenFetching>();
        var imapSubscriber = _serviceProvider.GetService<ImapSubcribe>();

        _stateMachine.Configure(driverInitialization, usernameEntering, TriggerEnum.Success);
        _stateMachine.Configure(usernameEntering, captureSolving, TriggerEnum.Success);
        _stateMachine.Configure(captureSolving, imapSubscriber, TriggerEnum.Success);
        _stateMachine.Configure(captureSolving, imapSubscriber, TriggerEnum.CaptureNotFound);

        _stateMachine.Configure(imapSubscriber, passwordEntering, TriggerEnum.Success);
        _stateMachine.Configure(passwordEntering, authenticatorCodeEntering, TriggerEnum.Success);
        _stateMachine.Configure(authenticatorCodeEntering, emailCodeEntering, TriggerEnum.Success);
        _stateMachine.Configure(authenticatorCodeEntering, emailCodeEntering, TriggerEnum.SkipAuthenticatorStep);
        _stateMachine.Configure(emailCodeEntering, tokenFetching, TriggerEnum.Success);
    }
}
