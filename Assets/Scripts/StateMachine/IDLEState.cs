using System;
using System.Collections.Generic;
using System.Threading;

public class IDLEState : IState
{
    private readonly Model _model;
    private readonly StateMachine _stateMachine;
    private readonly List<Type> _stateTypes;

    private CancellationTokenSource _cancellationTokenSource;

    public IDLEState(Model model, StateMachine stateMachine)
    {
        _model = model;
        _stateMachine = stateMachine;

        _stateTypes = new List<Type> { typeof(CurtainState), typeof(WashingState), typeof(HeadState) };
    }
    
    public void EnterState()
    {
        _model.SetAnimationTrigger("IDLE");
        _model.ChangeInteractionsBlock(false);

        _cancellationTokenSource?.Cancel();
        StateTimer();
    }

    public void ExitState()
    {
        _model.RemoveAnimationTrigger("IDLE");
        _model.RemoveAnimationTrigger("IDLE");
        _model.ChangeInteractionsBlock(true);
        
        _cancellationTokenSource?.Cancel();
    }

    public void Update()
    {
        _model.Update();
    }
    
    private async void StateTimer()
    {
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        
        await AsyncUtils.Instance.Wait((float)SettingsStorage.EventInterval.Value, cancellationToken);

        if (cancellationToken.IsCancellationRequested) return;
        
        _stateMachine.ChangeState(_stateTypes[UnityEngine.Random.Range(0, _stateTypes.Count)]);
    }
}