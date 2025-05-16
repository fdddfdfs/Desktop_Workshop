using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsView : MonoBehaviour
{
    [SerializeField] private Button _close;
    [SerializeField] private Slider _soundsVolume;
    [SerializeField] private TMP_Text _soundsVolumeValue;
    [SerializeField] private Slider _soundsInterval;
    [SerializeField] private TMP_Text _soundsIntervalValue;
    [SerializeField] private Slider _eventsInterval;
    [SerializeField] private TMP_Text _eventsIntervalValue;
    [SerializeField] private TMP_Dropdown _language;
    
    [SerializeField] private List<LanguageData> _languagesData;

    public event Action OnClose;
    public event Action<float> OnSoundsVolumeChange;
    public event Action<int> OnSoundsIntervalChange;
    public event Action<int> OnEventsIntervalChange;
    public event Action<int> OnLanguageChange;

    public IReadOnlyList<LanguageData> LanguagesData => _languagesData;
    
    public GraphicRaycaster LanguageDropdownRaycaster => !_language.gameObject.activeInHierarchy 
        ? null 
        : _language.GetComponentInChildren<GraphicRaycaster>();

    public void Init(Languages.Language language, float soundVolume, int soundInterval, int eventInterval)
    {
        _close.onClick.AddListener(() => OnClose?.Invoke());
        _soundsVolume.onValueChanged.AddListener(ChangeSoundsVolume);
        _soundsInterval.onValueChanged.AddListener(ChangeSoundsInterval);
        _eventsInterval.onValueChanged.AddListener(ChangeEventsInterval);
        _language.onValueChanged.AddListener(ChangeLanguage);
        
        _language.AddOptions(
            _languagesData.Select(data => new TMP_Dropdown.OptionData(data.LanguageName, data.Flag)).ToList());
        
        for (var i = 0; i < LanguagesData.Count; i++)
        {
            if (LanguagesData[i].Language == language)
            {
                _language.value = i;
                break;
            }
        }
        
        ChangeSoundsVolume(soundVolume);
        _soundsVolume.value = soundVolume;
        ChangeSoundsInterval(soundInterval);
        _soundsInterval.value = soundInterval;
        ChangeEventsInterval(eventInterval);
        _eventsInterval.value = eventInterval;
    }
    
    private void ChangeSoundsVolume(float value)
    {
        _soundsVolumeValue.text = $"{(int)(value * 100)}%";
        OnSoundsVolumeChange?.Invoke(value);
    }
    
    private void ChangeSoundsInterval(float value)
    {
        var intValue = (int)value;
        _soundsIntervalValue.text = intValue.ToString(CultureInfo.InvariantCulture);
        OnSoundsIntervalChange?.Invoke(intValue);
    }
    
    private void ChangeEventsInterval(float value)
    {
        var intValue = (int)value;
        _eventsIntervalValue.text = intValue.ToString(CultureInfo.InvariantCulture);
        OnEventsIntervalChange?.Invoke(intValue);
    }

    private void ChangeLanguage(int value)
    {
        OnLanguageChange?.Invoke(value);
    }
}