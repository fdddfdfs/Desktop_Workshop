using System;
using System.Windows.Forms;
using DG.Tweening;
using UnityEngine;
using Screen = UnityEngine.Screen;

public class CurtainView : MonoBehaviour
{
    private const float HideTime = 2f;
    
    [SerializeField] private GameObject _model;
    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private GameObject _additionalMesh;
    [SerializeField] private Camera _camera;
    
    private float _sizeX;
    private (Vector3 leftDown,Vector3 rightUp) _screenSize;
    private Vector3 _startScale;
    private float _currentScale;

    public void Appear(Vector3 position, float scale, Action onAppearComplete)
    {
        _currentScale = scale;
        _mesh.transform.localScale = _startScale * scale;
        _additionalMesh.transform.localScale = Vector3.one * scale;
        transform.position = position + Vector3.up * (_screenSize.rightUp.y * 3);
        transform.DOMoveY(position.y, HideTime).OnComplete(()=>
        {
            onAppearComplete?.Invoke();
        });
    }
    
    public void Hide()
    {
        transform.DOMoveY(_screenSize.leftDown.y * 3, HideTime);
    }

    public void Compress(float currentState)
    {
        _model.transform.localScale = new Vector3(currentState, 1, 1);
        _model.transform.localPosition = Vector3.right * (_sizeX * _currentScale * (1 - currentState));
    }
    
    private void Awake()
    {
        _sizeX = _mesh.bounds.size.x / 2;
        _screenSize = (
            _camera.ScreenToWorldPoint(Vector3.zero), 
            _camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)));
        _startScale = _mesh.transform.localScale;
    }
}