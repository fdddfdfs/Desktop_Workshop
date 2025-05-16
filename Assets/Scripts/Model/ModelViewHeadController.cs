using UnityEngine;
using UnityEngine.InputSystem;

public class ModelViewHeadController : MonoBehaviour
{
    private float _bodyWeight;
    private float _headWeight;
    private Camera _mainCamera;
    private Animator _animator;
    
    public void Init(Camera mainCamera, Animator animator, float bodyWeight, float headWeight)
    {
        _mainCamera = mainCamera;
        _bodyWeight = bodyWeight;
        _headWeight = headWeight;
        _animator = animator;
    }
    
    private void OnAnimatorIK(int layerIndex)
    {
        _animator.SetLookAtPosition(_mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()));
        _animator.SetLookAtWeight(1, _bodyWeight, _headWeight);
    }
}