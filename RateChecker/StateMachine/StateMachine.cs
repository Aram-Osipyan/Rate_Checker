using FluentMigrator.Builders.Update;
using RateChecker.Common;
using System.Runtime.CompilerServices;

namespace RateChecker.StateMachine;
public class StateMachine<TriggerEnum, Context, StateEnum> : IStateMachine<TriggerEnum, Context, StateEnum> where TriggerEnum : Enum
    where StateEnum : Enum
{
    private Dictionary<StateEnum, State<Context, TriggerEnum, StateEnum>> _statePool = new();
    private Dictionary<StateEnum, Dictionary<TriggerEnum, StateEnum>> _transactions = new();
    public void Configure(State<Context, TriggerEnum, StateEnum> from, State<Context, TriggerEnum, StateEnum> to, TriggerEnum trigger)
    {
        _statePool.TryAdd(from.stateEnum, from);
        _statePool.TryAdd(to.stateEnum, to);

        if (_transactions.TryGetValue(from.stateEnum, out var value))
        {
            value.TryAdd(trigger, to.stateEnum);
        }
        else
        {
            _transactions[from.stateEnum] = new Dictionary<TriggerEnum, StateEnum>
            {
                { trigger, to.stateEnum },
            };
        }
    }

    public async Task Run(StateEnum start, Context context)
    {
        var currentState = start;
        while (true)
        {
            var state = _statePool[currentState];

            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteLineAsync($"current state: {currentState} started");
            
            var trigger = await state.Perform(context);            

            await Console.Out.WriteLineAsync($"current state: {currentState}, result: {trigger}");


            Console.ForegroundColor = ConsoleColor.White;
            if (_transactions.TryGetValue(state.stateEnum, out var trans) && trans.TryGetValue(trigger, out var st))
            {
                currentState = st;
            }
            else
            {
                return;
            }
        }
    }
}
