using OpenQA.Selenium.DevTools.V113.Performance;

namespace RateChecker.StateMachine;
public abstract class State<Context, TriggerEnum, StateEnum> 
    where TriggerEnum : System.Enum                                                             
    where StateEnum : System.Enum
{
    public StateEnum stateEnum { get; set; }

    public State(StateEnum stateEnum)
    {
        this.stateEnum = stateEnum;
    }

    public abstract Task<TriggerEnum> Perform(Context context);

    public override int GetHashCode()
    {
        return this.stateEnum.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return 
            obj is not null &&
            obj is State<Context, TriggerEnum, StateEnum> &&
            this.stateEnum.Equals(((State<Context, TriggerEnum, StateEnum>)obj).stateEnum);
    }
}
