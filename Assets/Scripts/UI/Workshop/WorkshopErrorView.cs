using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopErrorView : MonoBehaviour
{
    [SerializeField] private TMP_Text _errorText;
    [SerializeField] private Button _closeButton;
    
    public event Action OnClose;
    
    public void UpdateErrorText(string error)
    {
        _errorText.text = error;
    }

    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnClose?.Invoke());
    }
}