using System.Threading;

public class HeadState : IState
{
    private readonly Model _model;
    private readonly StateMachine _stateMachine;

    public HeadState(Model model, StateMachine stateMachine)
    {
        _model = model;
        _stateMachine = stateMachine;
    }
    
    public void Update()
    {
        _model.Update();
    }

    public void EnterState()
    {
        Achievements.Instance.GetAchievement("Uwu");
        
        _model.ChangeInteractionsBlock(false);
        _model.MoveHeadBlendShapes(false);
        _model.PhraseWithCooldown("Uwu", true);
        StateTimer();
    }

    public void ExitState()
    {
        _model.ChangeInteractionsBlock(true);
        _model.MoveHeadBlendShapes(true);
    }

    private async void StateTimer()
    {
        CancellationToken token = AsyncUtils.Instance.GetCancellationToken();
        
        await AsyncUtils.Instance.Wait(10f, token);

        if (token.IsCancellationRequested) return;
        
        _stateMachine.ChangeState(typeof(IDLEState));
    } 
}