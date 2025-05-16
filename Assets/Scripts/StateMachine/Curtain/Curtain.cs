using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class Curtain : IUpdatable
{
    private const float Speed = 0.001f;
    private const float MinSize = 0.1f;
    private const float ReactDelayTime = 5f;
    private const float HitTimerWait = 3f;
    private const float ChangeStateDelay = 3f;

    private readonly CurtainView _curtainViewPrefab;
    private readonly Camera _camera;
    private readonly StateMachine _stateMachine;
    private readonly InputAction _mouseDelta;
    private readonly int _raycastMask;
    
    private CurtainView _curtainView;

    private bool _isMoving;
    private bool _isBlocked;

    private float _currentState = 1;
    
    private Dictionary<ReactionType, bool> _delays;
    private CancellationTokenSource _hitCancellationSource;

    public event Action<ReactionType> OnReaction;

    public Curtain(CurtainView curtainView, InputActionMap inputActionMap, Camera camera)
    {
        _curtainViewPrefab = curtainView;
        _camera = camera;
        InputAction leftMouse = inputActionMap["LeftMouse"];
        leftMouse.started += _ => StartMoving();
        leftMouse.canceled += _ => StopMoving();
        
        _mouseDelta = inputActionMap["MouseDelta"];
        
        _raycastMask = 1 << LayerMask.NameToLayer("Curtain");
        
        _curtainViewPrefab.gameObject.SetActive(false);

        _delays = new Dictionary<ReactionType, bool>();
    }

    private bool IsMouseOnModel()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        return Physics.Raycast(
            _camera.ScreenPointToRay(mousePosition).origin,
            _camera.ScreenPointToRay(mousePosition).direction,
            out _,
            Mathf.Infinity,
            _raycastMask);
    }
    
    public void Update()
    {
        if (_isMoving && !_isBlocked)
        {
            Move();
        }
    }

    public void Appear(Vector3 position, Vector3 scale, Action onAppearComplete)
    {
        _currentState = 1;

        if (_curtainView != null)
        {
            Object.Destroy(_curtainView.gameObject);
        }
        
        _curtainView = Object.Instantiate(_curtainViewPrefab).GetComponent<CurtainView>();
        _curtainView.gameObject.SetActive(true);
        _curtainView.Appear(position, scale.x, onAppearComplete);
    }

    public void Hide()
    {
        _hitCancellationSource?.Cancel();
        _curtainView.Hide();
    }
    
    public async void CloseCurtain()
    {
        _hitCancellationSource?.Cancel();
        _isBlocked = true;

        CancellationToken token = AsyncUtils.Instance.GetCancellationToken();

        float startState = _currentState;
        float time = 0;
        const float targetTime = 1f;

        while (_currentState < 1)
        {
            _currentState = Mathf.Clamp(startState + (1 - startState) * time / targetTime, 0, 1);
            _curtainView.Compress(_currentState);
            
            await AsyncUtils.Instance.WaitUntilNextFrame();
            
            time += Time.deltaTime;

            if (token.IsCancellationRequested) return;
        }

        await AsyncUtils.Instance.Wait(1f);

        if (token.IsCancellationRequested) return;

        OnReaction?.Invoke(ReactionType.CurtainClosed);

        _isBlocked = false;
    }

    private void Move()
    {
        var mouseDelta = _mouseDelta.ReadValue<Vector2>();
        float nextState = Mathf.Clamp(_currentState - mouseDelta.x * Speed, MinSize, 1);

        if (mouseDelta.x > 0)
        {
            if (nextState < 0.75f && _currentState >= 0.75 && !_delays.GetValueOrDefault(ReactionType.Warning, false))
            {
                ReactDelay(ReactionType.Warning);
                
                OnReaction?.Invoke(ReactionType.Warning);
            }
            else if (nextState < 0.5f && 
                     _currentState >= 0.5f && 
                     !_delays.GetValueOrDefault(ReactionType.IgnoreWarning, false))
            {
                ReactDelay(ReactionType.IgnoreWarning);
                HitTimer();
                
                OnReaction?.Invoke(ReactionType.IgnoreWarning);
            }
        }
        else
        {
            if (_hitCancellationSource is { IsCancellationRequested: false } && nextState > 0.75f)
            {
                _hitCancellationSource.Cancel();
            }
        }

        _currentState = nextState;
        
        _curtainView.Compress(_currentState);
        
    }

    private void StartMoving()
    {
        if (IsMouseOnModel())
        {
            _isMoving = true;
        }
    }

    private void StopMoving()
    {
        _isMoving = false;
    }

    private async void ReactDelay(ReactionType type)
    {
        _delays[type] = true;

        CancellationToken token = AsyncUtils.Instance.GetCancellationToken();

        await AsyncUtils.Instance.Wait(ReactDelayTime);

        if (token.IsCancellationRequested) return;

        _delays[type] = false;
    }

    private async void HitTimer()
    {
        _hitCancellationSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        
        CancellationToken token = _hitCancellationSource.Token;

        await AsyncUtils.Instance.Wait(HitTimerWait, token);

        if (token.IsCancellationRequested) return;
        
        OnReaction?.Invoke(ReactionType.Hit);
        CloseCurtain();
    }
}