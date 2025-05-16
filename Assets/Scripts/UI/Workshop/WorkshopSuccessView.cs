using System;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopSuccessView : MonoBehaviour
{
    [SerializeField] private Button _close;

    public event Action OnClose;

    private void Awake()
    {
        _close.onClick.AddListener(() => OnClose?.Invoke());
    }
}