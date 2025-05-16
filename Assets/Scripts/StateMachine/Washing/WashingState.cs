using System;
using System.Threading;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WashingState : IState
{
    private readonly Model _model;
    private readonly Washing _washing;

    private float _startRotationY;
    private Vector3 _startScale;
    
    public WashingState(Washing washing, Model model, StateMachine stateMachine)
    {
        _model = model;
        _washing = washing;
        _model.OnInWashing += () =>
        {
            _washing.ChangeInWashing(true);
            _model.PhraseWithCooldown("WashingMachineStuck", true);
        };

        _model.OnFromWashing += () =>
        {
            stateMachine.ChangeState(typeof(IDLEState));
        };
        
        _washing.OnRequiredClicks += ExitWashing;
        
        _startScale = _model.ViewTransform.localScale;
    }
    
    public void Update()
    {
    }

    public void EnterState()
    {
        Achievements.Instance.GetAchievement("Washing");

        
        Vector3 scale = _model.ViewTransform.localScale;
        Vector3 offset = Vector3.down * (0.25f * scale.y/ _startScale.y) + Vector3.forward * (3.8f * scale.y / _startScale.y);
        _washing.Appear(_model.ViewTransform.position + offset, scale.x, MoveToWashing);
        
        _startRotationY = _model.ModelTransform.rotation.eulerAngles.y;
        _model.ModelTransform.DORotate(Vector3.zero, WashingView.AnimationTime).SetEase(Ease.Linear);
    }
    
    public void ExitState()
    {
        _washing.CloseDoor();
        _washing.Hide();
        Vector3 scale = _model.ViewTransform.localScale;
        _model.ViewTransform
            .DOMove(_model.ViewTransform.position + Vector3.back * (3.8f * scale.y / _startScale.y), 1)
            .SetEase(Ease.Linear);
        
        _washing.ChangeInWashing(false);
        
        _model.PhraseWithCooldown("WashingMachineUnstuck", true);
        
        _model.ModelTransform
            .DORotate(Vector3.up*_startRotationY, WashingView.AnimationTime)
            .SetEase(Ease.Linear);
    }

    private void ExitWashing()
    {
        _model.SetAnimationTrigger("FromWashing");
    }

    private void MoveToWashing()
    {
        _model.ViewTransform.DOKill(true);
        _model.ModelTransform.DOKill(true);
        Vector3 scale = _model.ViewTransform.localScale;
        _model.ModelTransform.rotation = Quaternion.Euler(0, 180, 0);
        _model.ViewTransform.position += Vector3.forward * (3.8f * scale.y / _startScale.y);
        _washing.OpenDoor();
        _model.SetAnimationTrigger("InWashing");
    }
}