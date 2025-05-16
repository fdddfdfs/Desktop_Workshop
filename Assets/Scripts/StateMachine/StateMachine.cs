using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class StateMachine : IUpdatable
{
    private IState _currentState;

    private readonly Dictionary<Type, IState> _states;

    public StateMachine(Model model, Curtain curtain, Washing washing)
    {
        IDLEState idleState = new (model, this);
        CurtainState curtainState = new (model, curtain, this);
        WashingState washingState = new (washing, model, this);
        HeadState headState = new HeadState(model, this);

        _states = new Dictionary<Type, IState>
        {
            [typeof(IDLEState)] = idleState,
            [typeof(CurtainState)] = curtainState,
            [typeof(WashingState)] = washingState,
            [typeof(HeadState)] = headState,
        };

        ChangeState(typeof(IDLEState));
    }
    
    public void ChangeState(Type newState)
    {
        _currentState?.ExitState();
        _currentState = _states[newState];
        _currentState.EnterState();
    }

    public void Update()
    {
        _currentState.Update();
    }
}