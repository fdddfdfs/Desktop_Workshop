using System;
using System.Collections.Generic;
using DG.DemiLib;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private ModelData _modelData;
    [SerializeField] private InputActionAsset _input;
    [SerializeField] private MainMenuView _mainMenuView;
    [SerializeField] private CurtainView _curtainView;
    [SerializeField] private WashingView _washingView;
    [SerializeField] private WorkshopView _workshopView;
    [SerializeField] private WorkshopErrorView _workshopErrorView;
    [SerializeField] private WorkshopSuccessView _workshopSuccessView;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _uiCamera;
    [SerializeField] private PhrasesView _phrasesView;
    [SerializeField] private RequiredPhrasesData _requiredPhrasesData;
    [SerializeField] private List<PhrasesData> _phrasesData;
    [SerializeField] private SubtitlesView _subtitlesView;
    [SerializeField] private List<SubtitlesData> _subtitlesData;
    [SerializeField] private SettingsView _settingsView;

    private Model _model;
    private MainMenu _mainMenu;
    private Curtain _curtain;
    private Washing _washing;
    private StateMachine _stateMachine;
    private Subtitles _subtitles;

    public GraphicRaycaster GraphicRaycaster => _mainMenu.LanguageRaycaster;
    
    private void Awake()
    {
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        QualitySettings.vSyncCount = 1;
        Screen.SetResolution(Screen.width, Screen.height, false);
        
        InputActionMap inputMap = _input.FindActionMap("Input");
        
        _subtitles = new Subtitles(_subtitlesView, _subtitlesData);

        _model = new Model(new List<ModelData> { _modelData }, inputMap, _mainCamera, _uiCamera);
        _mainMenu = new MainMenu(
            _mainMenuView,
            _workshopView,
            _requiredPhrasesData,
            _phrasesView,
            _workshopErrorView,
            _workshopSuccessView,
            _settingsView,
            _phrasesData,
            _model);

        _model.OnLeftClick += _mainMenu.ChangeMenuActive;

        _curtain = new Curtain(_curtainView, inputMap, _mainCamera);
        _washing = new Washing(_washingView, _model, inputMap, _mainCamera);

        _stateMachine = new StateMachine(_model, _curtain, _washing);
        
        _model.PhraseWithCooldown("Greetings");
    }

    private void Update()
    {
        _stateMachine.Update();
    }
}