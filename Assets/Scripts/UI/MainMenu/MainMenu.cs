using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenu : MenuWithView<MainMenuView>
{
    private readonly Model _model;
    private readonly Workshop _workshop;
    private readonly Phrases _phrases;
    private readonly Settings _settings;
    
    private bool _soundMuted = false;
    private bool _isActive = false;

    public GraphicRaycaster LanguageRaycaster => _settings.LanguageRaycaster;
    
    public override bool IsActive => _isActive;

    private CancellationTokenSource _cancellationTokenSource;
    
    public MainMenu(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public MainMenu(
        MainMenuView tView,
        WorkshopView workshopView,
        RequiredPhrasesData requiredPhrasesData,
        PhrasesView phrasesView,
        WorkshopErrorView workshopErrorView,
        WorkshopSuccessView workshopSuccessView,
        SettingsView settingsView,
        List<PhrasesData> phrasesData,
        Model model) : base(tView)
    {
        _model = model;
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        
        _workshop = new Workshop(workshopView, requiredPhrasesData, workshopErrorView, workshopSuccessView);
        _phrases = new Phrases(phrasesView, requiredPhrasesData, phrasesData);
        _settings = new Settings(settingsView);

        _workshop.OnSubscribedItemDownloaded += 
            (folder) => Coroutines.StartRoutine(_phrases.AddNewPhrasesSet(folder));

        _workshop.StartDownloadingSubscribedItems();
        
        _view.OnExit += Exit;
        _view.OnMute += ChangeMuteStatus;
        _view.OnSettings += () =>
        {
            ChangeMenuActive();
            _settings.ChangeMenuActive(!_settings.IsActive, false);
            Achievements.Instance.GetAchievement("Settings");
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _view.OnWorkshop += () =>
        {
            ChangeMenuActive();
            _workshop.ChangeMenuActive(!_workshop.IsActive, false);
            Achievements.Instance.GetAchievement("Workshop");
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _view.OnPhrases += () =>
        {
            ChangeMenuActive();
            _phrases.ChangeMenuActive(!_phrases.IsActive, false);
            Achievements.Instance.GetAchievement("VoicePacks");
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _soundMuted = SettingsStorage.IsSoundMuted.Value == 1;
        _view.ChangeMuteImage(_soundMuted);
        float volume = _soundMuted ? 0 : SettingsStorage.SoundVolume.Value;
        Sounds.Instance.ChangeSoundsVolume(volume);
        
        _view.gameObject.SetActive(true);
    }

    public override void ChangeMenuActive()
    {
        Sounds.Instance.PlaySound(1, "Click");
        _isActive = !_isActive;
        _view.ChangeActiveWithAnimation(_isActive);

        if (!_isActive) return;
        
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 menuPosition = new Vector2(mousePosition.x * 1920 / Screen.width - 960f , (mousePosition.y - 540f) * 1920 / Screen.width);
        float lastMenuBorderX = _view.LastMenu.localPosition.x + _view.LastMenu.rect.width / 2;
        if (menuPosition.x + lastMenuBorderX > 960f)
        {
            menuPosition = new Vector2(960f - lastMenuBorderX, menuPosition.y);
        }
        _view.transform.localPosition = menuPosition;
    }

    private async void Exit()
    {
        Sounds.Instance.PlaySound(1, "Click");
        _model.PhraseWithCooldown("Goodbye", true);
        Achievements.Instance.GetAchievement("Exit");

        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());

        CancellationToken token = _cancellationTokenSource.Token;
        
        await AsyncUtils.Instance.Wait(3f, token);

        if (token.IsCancellationRequested)
        {
            return;
        }
        
        Application.Quit();
    }

    private async void ChangeMuteStatus()
    {
        Sounds.Instance.PlaySound(1, "Click");
        _soundMuted = !_soundMuted;
        
        _view.ChangeMuteImage(_soundMuted);
        
        if (!_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }
        
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());

        CancellationToken token = _cancellationTokenSource.Token;
        
        _model.PhraseWithCooldown(_soundMuted? "TurnOffSounds" : "TurnOnSounds", true);
        Achievements.Instance.GetAchievement(_soundMuted? "SoundsOff" : "SoundsOn");
        
        await AsyncUtils.Instance.Wait(3f, token);

        if (token.IsCancellationRequested)
        {
            return;
        }

        SettingsStorage.IsSoundMuted.Value = _soundMuted ? 1 : 0;
        float volume = _soundMuted ? 0 : SettingsStorage.SoundVolume.Value;
        Sounds.Instance.ChangeSoundsVolume(volume);
    }
}