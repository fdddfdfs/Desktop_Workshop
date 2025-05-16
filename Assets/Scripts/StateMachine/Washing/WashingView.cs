using System;
using DG.Tweening;
using UnityEngine;

public class WashingView : MonoBehaviour
{
    public const float AnimationTime = 2f;
    
    private static readonly int _openDoor = Animator.StringToHash("OpenDoor");
    private static readonly int _closeDoor = Animator.StringToHash("CloseDoor");
    
    [SerializeField] private GameObject _model;
    [SerializeField] private Animator _modelAnimator;
    [SerializeField] private Camera _camera;
    
    private Vector3 _startScale;

    private (Vector3 leftDown,Vector3 rightUp) _screenSize;
    
    public void Appear(Vector3 position, float scale, Action onAppearComplete)
    {
        _model.transform.localScale = _startScale * scale;
        transform.position = position + Vector3.up * _screenSize.rightUp.y * 3;
        transform.DOMoveY(position.y, AnimationTime).OnComplete(() => onAppearComplete?.Invoke());
    }
    
    public void Hide()
    {
        transform.DOMoveY(_screenSize.leftDown.y * 3, AnimationTime);
    }

    public void OpenDoor()
    {
        _modelAnimator.SetTrigger(_openDoor);
    }

    public void CloseDoor()
    {
        _modelAnimator.SetTrigger(_closeDoor);
    }
    
    private void Awake()
    {
        _screenSize = (
            _camera.ScreenToWorldPoint(Vector3.zero), 
            _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)));
        _startScale = _model.transform.localScale;
    }
}