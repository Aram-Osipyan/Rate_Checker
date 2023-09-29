using RateChecker.Common;

namespace RateChecker.StateMachine
{
    public interface IStateMachineFactory
    {
        StateMachine<TriggerEnum, StateMachineContext, StateEnum> GetStateMachine();
    }
}