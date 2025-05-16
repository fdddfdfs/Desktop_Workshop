using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings : AnimatedMenuWithView<SettingsView>
{
    public GraphicRaycaster LanguageRaycaster => _view.LanguageDropdownRaycaster; 
    
    public Settings(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public Settings(SettingsView tView) : base(tView)
    {
        _view.Init(
            (Languages.Language)SettingsStorage.Localization.Value,
            SettingsStorage.SoundVolume.Value,
            SettingsStorage.SoundInterval.Value,
            SettingsStorage.EventInterval.Value);
        
        Sounds.Instance.ChangeSoundsVolume(SettingsStorage.SoundVolume.Value);
        
        _view.OnLanguageChange += (value) =>
        {
            SettingsStorage.Localization.Value = (int)_view.LanguagesData[value].Language;
            Localization.Instance.ChangeLanguage(_view.LanguagesData[value].Language);
        };
        
        _view.OnSoundsVolumeChange += (value) =>
        {
            SettingsStorage.SoundVolume.Value = value;
            if (SettingsStorage.IsSoundMuted.Value == 1) value = 0;
            Sounds.Instance.ChangeSoundsVolume(value);
        };
        
        _view.OnSoundsIntervalChange += (value) => SettingsStorage.SoundInterval.Value = value;
        _view.OnEventsIntervalChange += (value) => SettingsStorage.EventInterval.Value = value;
        
        _view.OnClose += () =>
        {
            ChangeMenuActive(!IsActive, false);
            Sounds.Instance.PlaySound(1, "Click");
            Achievements.Instance.GetAchievement("CloseMenu");

        };
        
        _view.transform.localScale = Vector3.zero;
    }
}