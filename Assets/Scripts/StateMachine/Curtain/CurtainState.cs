using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class CurtainState : IState
{
    private const float StateTime = 20f;
    
    private readonly Model _model;
    private readonly Curtain _curtain;
    private readonly StateMachine _stateMachine;
    private readonly Action _changeReady;
    
    private bool _isReadyToUpdate;
    private Quaternion _startRotation;
    private float _startScale;

    private CancellationTokenSource _cancellationTokenSource;

    public CurtainState(Model model, Curtain curtain, StateMachine stateMachine)
    {
        _model = model;
        _curtain = curtain;
        _stateMachine = stateMachine;
        _startScale = _model.ModelTransform.localScale.x;
        _changeReady = () =>
        {
            _isReadyToUpdate = true;
            _model.ModelTransform.position += Vector3.forward / 2 * _model.ModelTransform.localScale.x / _startScale;
            _model.ModelTransform.rotation = Quaternion.Euler(0, 180, 0);
        };

        _curtain.OnReaction += Reaction;
    }
    
    public void Update()
    {
        if (!_isReadyToUpdate) return;
        
        _curtain.Update();
    }

    public void EnterState()
    {
        Achievements.Instance.GetAchievement("Curtain");
        
        _isReadyToUpdate = false;

        Vector3 scale = _model.ViewTransform.localScale;
        Vector3 offset = Vector3.up * _model.ModelSize.y / 2 +
                         Vector3.back * _model.ModelTransform.localScale.x / _startScale;
        _curtain.Appear(_model.ViewTransform.position + offset, scale, _changeReady);
        
        _startRotation = _model.ModelTransform.rotation;
        
        StateTimer();
        _model.PhraseWithCooldown("ClothesChanging", true);
    }

    public void ExitState()
    {
        _curtain.Hide();
        _model.PhraseWithCooldown("ClothesChangingEnd", true);
        _model.ModelTransform.position += Vector3.back / 2 * _model.ModelTransform.localScale.x / _startScale;;
    }

    private void Reaction(ReactionType reactionType)
    {
        switch (reactionType)
        {
            case ReactionType.Warning:
                Debug.Log("Warning");
                _model.ChangeClothes(5);
                _model.SetAnimationTrigger("Hide");
                _model.PhraseWithCooldown("ClothesWarning", true);
                break;
            case ReactionType.IgnoreWarning:
                Debug.Log("Ignore Warning");
                _model.PhraseWithCooldown("ClothesIgnoreWarning", true);
                break;
            case ReactionType.Hit:
                Debug.Log("Hit");
                _model.SetAnimationTrigger("Hit");
                _cancellationTokenSource.Cancel();
                break;
            case ReactionType.CurtainClosed:
                _model.ChangeClothes();
                _stateMachine.ChangeState(typeof(IDLEState));
                _model.ModelTransform.rotation = _startRotation;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reactionType), reactionType, null);
        }
    }

    private async void StateTimer()
    {
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        
        CancellationToken token = _cancellationTokenSource.Token;

        await AsyncUtils.Instance.Wait(StateTime, token);

        if (token.IsCancellationRequested) return;
        
        _curtain.CloseCurtain();
    }
}