using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class Model : IUpdatable
{
    private const float Speed = 0.001f;
    private const float MinimumScale = 0.5f;
    private const float RotationSpeed = 0.5f;
    private const float MaximumRotationForClick = 3f;

    private readonly Camera _camera;
    private readonly Camera _uiCamera;
    private readonly LayerMask _raycastMask;
    private readonly Dictionary<ModelData, ModelView> _models;
    private readonly Vector2 _borders;
    private readonly InputAction _scroll;
    private readonly Animator _animator;
    private readonly InputAction _mouseDelta;

    private bool _isInteractionsBlocked;
    private bool _isMoving;
    private bool _isRotating;
    private ModelView _currentModel;
    private Vector2 _movingOffset;
    private AnimationClip _currentClip;
    private float _currentAnimationSpeed;

    private Quaternion _startRotation;
    
    private bool _phraseCooldown;
    private CancellationTokenSource _phraseCooldownCancellationTokenSource;
    private CancellationTokenSource _randomPhrasesCancellationTokenSource;

    public ModelData CurrentModelData { get; private set; }

    public Transform ViewTransform => _currentModel.transform;
    public Transform ModelTransform => _currentModel.ModelTransform;

    public Vector3 ModelSize => _currentModel.Size;

    public event Action OnLeftClick;
    public event Action OnInWashing;
    public event Action OnFromWashing;
    
    public Model(List<ModelData> modelDatas, InputActionMap inputActionMap, Camera camera, Camera uiCamera)
    {
        _camera = camera;
        _uiCamera = uiCamera;
        _raycastMask = 1 << LayerMask.NameToLayer("Model");

        InputAction leftMouse = inputActionMap["LeftMouse"];
        leftMouse.started += _ => StartMoving();
        leftMouse.canceled += _ => StopMoving();

        InputAction rightMouse = inputActionMap["RightMouse"];
        rightMouse.started += _ => StartRotation();
        rightMouse.canceled += _ => StopRotation();

        _scroll = inputActionMap["Scroll"];
        _mouseDelta = inputActionMap["MouseDelta"];

        _models = new Dictionary<ModelData, ModelView>();
        foreach (ModelData modelData in modelDatas)
        {
            var modelView = ResourcesLoader.InstantiateLoadedComponent<ModelView>(modelData.Prefab);
            _models[modelData] = modelView;
            modelView.ChangeActive(false);
            modelView.BoxCollider.enabled = false;
            modelView.Init(_uiCamera);
        }

        _currentModel = _models[modelDatas[0]];
        _currentModel.ChangeActive(true);
        _currentModel.BoxCollider.enabled = true;
        CurrentModelData = modelDatas[0];
        //Achievements.Instance.GetAchievement(modelDatas[0].AchievementData);
        Rotate();

        _borders = new Vector2(Screen.width, Screen.height);
        _borders = _uiCamera.ScreenToWorldPoint(_borders);

        //PlayRandomPhrase();

        _currentModel.transform.position = new Vector3(0, -3, 0);

        _animator = _currentModel.Animator;

        _currentModel.ChangeClothes(0);
        
        var inWashingBehaviour = _animator.GetBehaviour<InWashingBehaviour>();
        inWashingBehaviour.OnExit += () => OnInWashing?.Invoke();
        
        var fromWashingBehaviour = _animator.GetBehaviour<FromWashingBehaviour>();
        fromWashingBehaviour.OnExit += () => OnFromWashing?.Invoke();
    }

    public void MoveHeadBlendShapes(bool isReversed)
    {
        _currentModel.MoveHeadBlendShapes(isReversed);
    }

    public void SetAnimationTrigger(string trigger)
    {
        _animator.SetTrigger(trigger);
    }

    public void RemoveAnimationTrigger(string trigger)
    {
        _animator.ResetTrigger(trigger);
    }

    public bool IsMouseOnModel()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        //Ray ray = _camera.ScreenPointToRay(mousePosition);
        
        return Physics.Raycast(
            _camera.ScreenPointToRay(mousePosition).origin,
            _camera.ScreenPointToRay(mousePosition).direction,
            out _,
            Mathf.Infinity,
            _raycastMask);
        
        //return Physics.Raycast(ray, out RaycastHit _, Mathf.Infinity, _raycastMask);
    }

    public void Update()
    {
        if (_isInteractionsBlocked) return;
        
        if (_isMoving)
        {
            Move();
            Achievements.Instance.GetAchievement("Moving");
        }

        if (_isRotating)
        {
            Rotate();
            Achievements.Instance.GetAchievement("Rotating");

        }

        float scrollDeltaY = _scroll.ReadValue<Vector2>().y;

        if (scrollDeltaY != 0 && IsMouseOnModel())
        {
            ChangeScale(scrollDeltaY);
            PhraseWithCooldown("Scaling");
            Achievements.Instance.GetAchievement("Scaling");

        }
    }

    public void ChangeClothes(int index = -1)
    {
        _currentModel.ChangeClothes(index);
    }

    public void ChangeInteractionsBlock(bool isBlocked)
    {
        _isInteractionsBlocked = isBlocked;

        if (isBlocked)
        {
            _randomPhrasesCancellationTokenSource?.Cancel();
        }
        else
        {
            _randomPhrasesCancellationTokenSource = 
                CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
            PlayRandomPhrase();
        }
    }
    
    public void PhraseWithCooldown(string phrase, bool ignoreCooldown = false)
    {
        if (_phraseCooldown && !ignoreCooldown) return;

        Sounds.Instance.PlaySound(0, phrase);

        PhraseCooldown();
    }

    private void Move()
    {
        Vector2 newPosition = (Vector2)_uiCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()) 
                              + _movingOffset; 
        _currentModel.transform.position = GetClampedPosition(newPosition);
    }

    private Vector2 GetClampedPosition(Vector2 position)
    {
        if (Mathf.Abs(position.x) + _currentModel.Size.x / 2 > _borders.x)
        {
            position = new Vector2(
                Mathf.Sign(position.x) * (_borders.x - _currentModel.Size.x / 2),
                position.y);
        }
            
        if (position.y + _currentModel.Size.y > _borders.y)
        {
            position = new Vector2(
                position.x,
                _borders.y - _currentModel.Size.y);
        }
        else if (position.y < -_borders.y)
        {
            position = new Vector2(position.x, -_borders.y);
        }

        return position;
    }

    private void ChangeScale(float delta)
    {
        delta *= Speed;
        
        if (_currentModel.Size.y + delta > _borders.y * 2) return;
        if (_currentModel.Size.y + delta < _borders.y * MinimumScale) return;

        _currentModel.transform.localScale += Vector3.one * delta;
        _currentModel.transform.position = GetClampedPosition(_currentModel.transform.position);
    }

    private void Rotate()
    {
        var mouseDelta = _mouseDelta.ReadValue<Vector2>();
        _currentModel.ModelTransform.transform.rotation *= Quaternion.Euler(0, -mouseDelta.x * RotationSpeed, 0);

        _currentModel.ChangeHairsBoneGravity(_currentModel.ModelTransform.rotation * 
                                             Quaternion.Euler(0,180,0) *
                                             CurrentModelData.HairsBoneGravity);
    }

    private void StartMoving()
    {
        if (_isInteractionsBlocked) return;
        
        if (IsMouseOnModel())
        {
            _isMoving = true;
            _movingOffset = _currentModel.transform.position 
                            - _uiCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
            PhraseWithCooldown("Moving");
        }
    }

    private void StopMoving()
    {
        _isMoving = false;
    }

    private void StartRotation()
    {
        if (_isInteractionsBlocked) return;
        
        if (IsMouseOnModel())
        {
            _isRotating = true;
            _startRotation = _currentModel.ModelTransform.transform.rotation;
            PhraseWithCooldown("Rotating");
        }
    }

    private void StopRotation()
    {
        if (_isRotating)
        {
            Quaternion currentRotation = _currentModel.ModelTransform.transform.rotation;
            float difference = Mathf.Abs(_startRotation.eulerAngles.y - currentRotation.eulerAngles.y);

            if (difference < MaximumRotationForClick)
            {
                OnLeftClick?.Invoke();
            }
        }
        else
        {
            OnLeftClick?.Invoke();
        }
        
        _isRotating = false;
    }

    private async void PlayRandomPhrase()
    {
        CancellationToken token = _randomPhrasesCancellationTokenSource.Token;
        
        await AsyncUtils.Instance.Wait(SettingsStorage.SoundInterval.Value);

        if (token.IsCancellationRequested) return;

        if (!_phraseCooldown)
        {
            Sounds.Instance.PlayRandomSounds(0, "IDLE");
            
            PhraseCooldown();
        }

        PlayRandomPhrase();
    }

    private async void PhraseCooldown()
    {
        _phraseCooldown = true;

        _phraseCooldownCancellationTokenSource?.Cancel();
        _phraseCooldownCancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        CancellationToken token = _phraseCooldownCancellationTokenSource.Token;

        await AsyncUtils.Instance.Wait((float)SettingsStorage.SoundInterval.Value, token);

        if (token.IsCancellationRequested) return;

        _phraseCooldown = false;
    }
}