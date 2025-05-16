using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public sealed class Localization
{
    public const Languages.Language DefaultLanguage = Languages.Language.English;

    private const string Path = "Localization/";

    private static Localization _instance;

    private InGameTextLocalization _defaultLocalization;
    private Dictionary<string, string> _localization;
    private bool _initialized;

    public event Action OnLanguageUpdated;

    public Languages.Language CurrentLanguage { get; private set; }

    public static Localization Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Localization();
                _instance.Initialize();
            }

            return _instance;
        }
    }

    public string this[string key]
    {
        get
        {
            if (key == null) return "Tried to get translation for null key";
            
            if (_localization.TryGetValue(key, out string item))
            {
                return item;
            }
            
            Debug.LogWarning($"Localization don`t contains translation for key: {key}");

            return key;
        }
    }

    public void ResetBindings()
    {
        Instance.OnLanguageUpdated = null;
    }

    public void ChangeLanguage(Languages.Language newLanguage)
    {
        if (CurrentLanguage == newLanguage && _initialized) return;
        
        InGameTextLocalization newLanguageLocalization = LoadLocalization(newLanguage);

        for (var i = 0; i < _defaultLocalization.Texts.Length; i++)
        {
            string text = _defaultLocalization.Texts[i];
            string localizedText = newLanguageLocalization.Texts[i];
            _localization[text] = localizedText;
        }

        CurrentLanguage = newLanguage;

        SettingsStorage.Localization.Value = (int)CurrentLanguage;
        
        if (_initialized)
        {
            OnLanguageUpdated?.Invoke();
        }
    }

    private static InGameTextLocalization LoadLocalization(Languages.Language language)
    {
        var path = $"{Path}{Languages.SteamJsonLanguages[language]}";
        var targetFile = Resources.Load<TextAsset>(path);

        if (targetFile == null)
        {
            throw new Exception($"Resources doesnt contains localization file {path} for language {language}");
        }
        
        return JsonConvert.DeserializeObject<InGameTextLocalization>(targetFile.text);
    }

    private void Initialize()
    {
        var playerLanguage = (Languages.Language)SettingsStorage.Localization.Value;
        _defaultLocalization = LoadLocalization(DefaultLanguage);
        _localization = new Dictionary<string, string>();
        ChangeLanguage(playerLanguage);
        _initialized = true;
    }
}