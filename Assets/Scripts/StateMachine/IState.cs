public interface IState : IUpdatable
{
    public void EnterState();

    public void ExitState();
}