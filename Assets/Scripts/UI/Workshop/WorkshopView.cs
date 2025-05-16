using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkshopView : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleHeader;
    [SerializeField] private TMP_InputField _titleInput;
    [SerializeField] private TMP_Text _descriptionHeader;
    [SerializeField] private TMP_InputField _descriptionInput;
    [SerializeField] private TMP_Text _languageHeader;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private TMP_Text _contentFolderHeader;
    [SerializeField] private TMP_InputField _contentFolderInput;
    [SerializeField] private TMP_Text _uploadHeader;
    [SerializeField] private Button _uploadButton;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _helpButton;

    [SerializeField] private List<LanguageData> _languagesData;

    [SerializeField] private GameObject _locker;
    [SerializeField] private TMP_Text _lockerText;
    
    private Dictionary<string, LanguageData> _languageDataDictionary;
    
    private CancellationTokenSource _lockerCancellationTokenSource;
    private string[] _lockerStartTextVariants;
    private string _lockerStartText;

    public event Action OnContentFolderSelect;
    public event Action<WorkshopData> OnUploadButtonClick;
    public event Action OnClose;
    public event Action OnHelpButtonClick;
    public event Action<string> OnError;

    public void UpdateContentFolder(string path)
    {
        _contentFolderInput.text = path;
    }

    public void ClearFields()
    {
        _titleInput.text = "";
        _descriptionInput.text = "";
        _contentFolderInput.text = "";
    }

    public void LockView()
    {
        _locker.SetActive(true);
        _lockerCancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        AnimateLockText();
    }

    public void UnlockView()
    {
        _lockerCancellationTokenSource.Cancel();
        _lockerText.text = _lockerStartTextVariants[0];
        _locker.SetActive(false);
    }
    
    private void Awake()
    {
        _closeButton.onClick.AddListener(() => OnClose?.Invoke());
        _contentFolderInput.onSelect.AddListener((_) => OnContentFolderSelect?.Invoke());
        _helpButton.onClick.AddListener(() => OnHelpButtonClick?.Invoke());
        _uploadButton.onClick.AddListener(
            () =>
            {
                bool result = ValidateData();

                if (result)
                {
                    OnUploadButtonClick?.Invoke(new WorkshopData(
                        _titleInput.text,
                        _descriptionInput.text,
                        _languageDataDictionary[_languageDropdown.options[_languageDropdown.value].text].Language,
                        _contentFolderInput.text));
                }
                else
                {
                    OnError?.Invoke("All fields should be filled");
                }
            });
        
        _languageDropdown.AddOptions(
            _languagesData.Select(data => new TMP_Dropdown.OptionData(data.LanguageName, data.Flag)).ToList());
        
        _languageDataDictionary = new Dictionary<string, LanguageData>();
        foreach (LanguageData languageData in _languagesData)
        {
            _languageDataDictionary.Add(languageData.LanguageName, languageData);
        }

        _lockerStartTextVariants = new string[4];
        Localization.Instance.OnLanguageUpdated += CreateLockerStartTextsVariants;
        _lockerStartText = _lockerText.text;
        CreateLockerStartTextsVariants();
    }

    private void CreateLockerStartTextsVariants()
    {
        _lockerStartTextVariants[0] = Localization.Instance[_lockerStartText];

        for (var i = 1; i < _lockerStartTextVariants.Length; i++)
        {
            _lockerStartTextVariants[i] = _lockerStartTextVariants[i - 1] + '.';
        }
    }

    private async void AnimateLockText()
    {
        CancellationToken cancellationToken = _lockerCancellationTokenSource.Token;

        int counter = 0;
        
        while (!cancellationToken.IsCancellationRequested)
        {
            _lockerText.text = _lockerStartTextVariants[counter % 3 + 1];
            await AsyncUtils.Instance.Wait(0.5f, cancellationToken);
            counter++;
        }
    }

    private bool ValidateData()
    {
        if(_titleInput.text != string.Empty &&
           _descriptionInput.text != string.Empty &&
           _contentFolderInput.text != string.Empty)
        {
            return true;
        }
        
        return false;
    }
    
    public struct WorkshopData
    {
        public readonly string Title;
        public readonly string Description;
        public readonly Languages.Language Language;
        public readonly string ContentFolderPath;
        
        public WorkshopData(string title, string description, Languages.Language language, string contentFolderPath)
        {
            Title = title;
            Description = description;
            Language = language;
            ContentFolderPath = contentFolderPath;
        }
    }
}