namespace RateChecker.StateMachine
{
    public interface IStateMachine<TriggerEnum, Context, StateEnum>
        where TriggerEnum : Enum
        where StateEnum : Enum
    {
        void Configure(State<Context, TriggerEnum, StateEnum> from, State<Context, TriggerEnum, StateEnum> to, TriggerEnum trigger);
        Task Run(StateEnum start, Context context);
    }
}