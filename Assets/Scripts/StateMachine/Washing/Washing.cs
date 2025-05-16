using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Washing
{
    private const int RequiredClicks = 5;
    private const float ClickTimerDuration = 60;
    
    private readonly WashingView _washingView;
    private readonly Model _model;
    private readonly Camera _camera;
    private readonly int _raycastMask;

    private bool _isInWashing;
    private int _clickCount;
    private bool _isClickBlocked;
    private CancellationTokenSource _cancellationTokenSource;
    private float _startScale;

    public event Action OnRequiredClicks;

    public Washing(WashingView washingView, Model model, InputActionMap inputActionMap, Camera camera)
    {
        _washingView = washingView;
        _model = model;
        _startScale = _model.ModelTransform.localScale.x;
        _camera = camera;
        InputAction leftMouse = inputActionMap["LeftMouse"];
        leftMouse.started += _ => Click();
        
        _washingView.gameObject.SetActive(false);
        
        _raycastMask = LayerMask.GetMask("Model", "Washing");
    }

    public void Appear(Vector3 position, float scale, Action onAppearComplete)
    {
        _washingView.gameObject.SetActive(true);
        _washingView.Appear(position, scale, onAppearComplete);
    }

    public void Hide()
    {
        _washingView.Hide();
    }

    public void OpenDoor()
    {
        _washingView.OpenDoor();
    }

    public void CloseDoor()
    {
        _washingView.CloseDoor();
    }

    public void ChangeInWashing(bool state)
    {
        _isInWashing = state;

        if (state)
        {
            _clickCount = 0;
            _cancellationTokenSource = 
                CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
            ClickTimer();
        }
    }
    
    private void Click()
    {
        if (!_isInWashing || _isClickBlocked || _clickCount == RequiredClicks) return;

        if (!IsMouseOnModel()) return;

        _clickCount++;
        
        MoveModel();

        if (_clickCount == RequiredClicks)
        {
            _cancellationTokenSource.Cancel();
            OnRequiredClicks?.Invoke();
        }
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

    private void MoveModel()
    {
        _isClickBlocked = true;
        
        _model.ViewTransform.DOKill(true);
        Vector3 position = _model.ViewTransform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_model.ViewTransform.DOMoveZ(
            position.z - 1 * _model.ModelTransform.localScale.x / _startScale,
            0.25f));
        sequence.Append(_model.ViewTransform.DOMoveZ(position.z, 0.25f));
        sequence.SetEase(Ease.Linear);
        sequence.OnComplete(() => _isClickBlocked = false);
    }

    private async void ClickTimer()
    {
        CancellationToken token = _cancellationTokenSource.Token;
        
        await AsyncUtils.Instance.Wait(ClickTimerDuration * 0.1f, token);
        
        if (token.IsCancellationRequested) return;
        
        _model.PhraseWithCooldown("WashingMachineHelp", true);
        
        await AsyncUtils.Instance.Wait(ClickTimerDuration * 0.9f, token);
        
        if (token.IsCancellationRequested) return;
        
        OnRequiredClicks?.Invoke();
    }
}