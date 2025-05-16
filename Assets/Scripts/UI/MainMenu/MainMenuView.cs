using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    private const float AnimationDuration = 0.25f;
    
    [SerializeField] private Button _settings;
    [SerializeField] private Button _workshop;
    [SerializeField] private Button _phrases;
    [SerializeField] private Button _exit;
    [SerializeField] private Button _mute;
    [SerializeField] private Image _muteImage;
    [SerializeField] private Sprite _soundOn;
    [SerializeField] private Sprite _soundOff;
    [SerializeField] private List<RectTransform> _parents;

    private float _targetScale;

    public event Action OnExit;
    public event Action OnMute;
    public event Action OnSettings;
    public event Action OnWorkshop;
    public event Action OnPhrases;

    public RectTransform LastMenu => _parents[^1];

    public void ChangeMuteImage(bool isOn)
    {
        _muteImage.sprite = isOn ? _soundOn : _soundOff;
    }

    public void ChangeActiveWithAnimation(bool state)
    {
        foreach (RectTransform parent in _parents)
        {
            parent.transform.DOKill(true);

            if (state)
            {
                parent.gameObject.SetActive(true);
                parent.transform.localScale = Vector3.zero;
                parent.transform.DOScale(_targetScale, AnimationDuration)
                    .SetEase(Ease.OutBounce);
            }
            else
            {
                parent.transform.localScale = Vector3.one * _targetScale;
                parent.transform.DOScale(0, AnimationDuration)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() => parent.gameObject.SetActive(false));
            }
        }
    }
    
    private void Awake()
    {
        _exit.onClick.AddListener(() => OnExit?.Invoke());
        _mute.onClick.AddListener(() => OnMute?.Invoke());
        _settings.onClick.AddListener(() => OnSettings?.Invoke());
        _workshop.onClick.AddListener(() => OnWorkshop?.Invoke());
        _phrases.onClick.AddListener(() => OnPhrases?.Invoke());

        if (_parents == null || _parents.Count == 0) return;
        
        _targetScale = _parents[0].transform.localScale.x;

        foreach (RectTransform parent in _parents)
        {
            parent.gameObject.SetActive(false);
        }
    }
}