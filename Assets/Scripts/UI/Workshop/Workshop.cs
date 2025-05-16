using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Steamworks;
using UnityEngine;

public class Workshop : AnimatedMenuWithView<WorkshopView>
{
    private readonly RequiredPhrasesData _requiredPhrasesData;
    private readonly CallResult<CreateItemResult_t> _createItemResult;
    private readonly CallResult<SubmitItemUpdateResult_t> _submitItemUpdateResult;
    private readonly Callback<DownloadItemResult_t> _downloadItemResult;
    
    private readonly AppId_t _appId;

    private readonly Dictionary<Languages.Language, string> _steamLanguages;
    
    private readonly WorkshopError _workshopError;
    private readonly WorkshopSuccess _workshopSuccess;
    
    private WorkshopView.WorkshopData _currentWorkshopData;
    
    public event Action<string> OnSubscribedItemDownloaded;
    
    public Workshop(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public Workshop(
        WorkshopView tView,
        RequiredPhrasesData requiredPhrasesData,
        WorkshopErrorView workshopErrorView,
        WorkshopSuccessView workshopSuccessView) : base(tView)
    {
        _requiredPhrasesData = requiredPhrasesData;

        if (SteamManager.Initialized)
        {
            _appId = SteamUtils.GetAppID();
            _createItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
            _submitItemUpdateResult = CallResult<SubmitItemUpdateResult_t>.Create(OnSubmitItemUpdate);
            _downloadItemResult = Callback<DownloadItemResult_t>.Create(OnItemDownloaded);
        }

        _steamLanguages = new Dictionary<Languages.Language, string>();
        foreach (string key in Languages.SteamUILanguages.Keys)
        {
            _steamLanguages.Add(Languages.SteamUILanguages[key], key);
        }
        
        _view.OnContentFolderSelect += ChooseContentDirectory;
        _view.OnUploadButtonClick += CreateNewItem;
        _view.OnClose += () =>
        {
            ChangeMenuActive(false, false);
            Achievements.Instance.GetAchievement("CloseMenu");
            Sounds.Instance.PlaySound(1, "Click");

        };
        
        _view.OnError += (error) => _workshopError?.SetErrorText(error);
        _view.OnHelpButtonClick += () =>
        {
            SteamFriends.ActivateGameOverlayToWebPage(
                "https://steamcommunity.com/sharedfiles/filedetails/?id=3475452111");
            Achievements.Instance.GetAchievement("Question");
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _workshopError = new WorkshopError(workshopErrorView);
        _workshopSuccess = new WorkshopSuccess(workshopSuccessView);
        
        _view.transform.localScale = Vector3.zero;
    }
    
    public void StartDownloadingSubscribedItems()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steam manager don`t initialized, cannot download subscribed items");
            return;
        }
        
        DownloadSubscribedItems();
    }

    private void DownloadSubscribedItems()
    {
        var items = new PublishedFileId_t[100];
        uint itemsCount = SteamUGC.GetSubscribedItems(items, (uint)items.Length);

        for (var i = 0; i < itemsCount; i++)
        {
            ulong itemState = SteamUGC.GetItemState(items[i]);
            bool isInstalled = (itemState & (uint)EItemState.k_EItemStateInstalled) != 0;
            bool isNeedsUpdate = (itemState & (uint)EItemState.k_EItemStateNeedsUpdate) != 0;
            bool isDownloading = (itemState & (uint)EItemState.k_EItemStateDownloading) != 0;
            bool isSubscribed = (itemState & (uint)EItemState.k_EItemStateSubscribed) != 0;
            bool isNone = (itemState & (uint)EItemState.k_EItemStateNone) != 0;

            if ((isInstalled && isNeedsUpdate) ||(!isInstalled && isSubscribed) || (!isInstalled && isNone))
            {
                bool result = SteamUGC.DownloadItem(items[i], false);
                Debug.Log($"Start download item {items[i]} result: {result}");
            }
            else if(isDownloading)
            {
                Debug.Log("Already downloading item");
            }
            else if (isInstalled)
            {
                GetDownloadedItemFolder(items[i]);
            }
        }
    }
    
    private void OnItemDownloaded(DownloadItemResult_t result)
    {
        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"DownloadItem failed: {result.m_eResult}");
            return;
        }
        
        Debug.Log("Item downloaded successfully");

        if (result.m_unAppID != _appId)
            return;
        
        GetDownloadedItemFolder(result.m_nPublishedFileId);
    }

    private void GetDownloadedItemFolder(PublishedFileId_t item)
    {
        bool result = 
            SteamUGC.GetItemInstallInfo(item, out ulong size, out string folder, 1024, out uint timeStamp);

        if (!result)
        {
            Debug.LogError($"Failed to parse item {item}");
        }
        else
        {
            Debug.Log($"{item} downloaded to {folder}");
        }
        
        OnSubscribedItemDownloaded?.Invoke(folder);
    }

    private void CreateNewItem(WorkshopView.WorkshopData workshopData)
    {
        if (!SteamManager.Initialized)
        {
            _workshopError.SetErrorText(Localization.Instance[AllTexts.NoConnection]);
            return;
        }
        
        Debug.Log("Create new item");
        
        _currentWorkshopData = workshopData;
        
        SteamAPICall_t call = SteamUGC.CreateItem(_appId, EWorkshopFileType.k_EWorkshopFileTypeCommunity);
        _createItemResult.Set(call);
    }

    private void OnCreateItemResult(CreateItemResult_t result, bool bIOFailure)
    {
        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"CreateItem failed: {result.m_eResult}");
            _workshopError.SetErrorText(Localization.Instance[AllTexts.FailedToCreate]);
            return;
        }
        
        Debug.Log("Item created successfully");

        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            SteamFriends.ActivateGameOverlayToWebPage(
                $"steam://url/CommunityFilePage/{result.m_nPublishedFileId.m_PublishedFileId}");
        }
        
        StartItemUpdate(result.m_nPublishedFileId);
    }

    private void OnSubmitItemUpdate(SubmitItemUpdateResult_t result, bool bIOFailure)
    {
        if (result.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"SubmitItemUpdate failed: {result.m_eResult}");
            _workshopError.SetErrorText(Localization.Instance[AllTexts.FailedToSubmit]);
        }
        else
        {
            Debug.Log("Item submitted successfully");
            _workshopSuccess.ChangeMenuActive(true, false);
        }
        
        _view.UnlockView();
        _view.ClearFields();

        if (result.m_bUserNeedsToAcceptWorkshopLegalAgreement)
        {
            SteamFriends.ActivateGameOverlayToWebPage(
                $"steam://url/CommunityFilePage/{result.m_nPublishedFileId.m_PublishedFileId}");
        }
    }

    private void StartItemUpdate(PublishedFileId_t publishedFileId)
    {
        UGCUpdateHandle_t updateHandle = SteamUGC.StartItemUpdate(_appId, publishedFileId);
        
        if (updateHandle == UGCUpdateHandle_t.Invalid)
        {
            Debug.LogError("Failed to start item update");
            _workshopError.SetErrorText(Localization.Instance[AllTexts.FailedToUpdate]);
            return;
        }
        
        SteamUGC.SetItemTitle(updateHandle, _currentWorkshopData.Title);
        SteamUGC.SetItemDescription(updateHandle, _currentWorkshopData.Description);
        SteamUGC.SetItemUpdateLanguage(updateHandle, _steamLanguages[_currentWorkshopData.Language]);
        SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
        SteamUGC.SetItemContent(updateHandle, _currentWorkshopData.ContentFolderPath);
        SteamUGC.SetItemPreview(updateHandle, GetPreviewPath(_currentWorkshopData.ContentFolderPath));
        
        SteamAPICall_t call = SteamUGC.SubmitItemUpdate(updateHandle, null);
        _submitItemUpdateResult.Set(call);
        
        _view.LockView();
    }

    private void ChooseContentDirectory()
    {
        using FolderBrowserDialog folderBrowser = new ();
        
        folderBrowser.Description = "Select a folder";
        folderBrowser.ShowNewFolderButton = true;
        if (folderBrowser.ShowDialog() == DialogResult.OK)
        {
            string selectedPath = folderBrowser.SelectedPath;
            Debug.Log("Selected Directory: " + selectedPath);
            
            bool isValid = ValidateDirectory(selectedPath);

            if (isValid)
            {
                _view.UpdateContentFolder(selectedPath);
            }
            else
            {
                Debug.Log("Invalid Directory");
            }
        }
        else
        {
            Debug.Log("No folder selected");
        }
    }

    private bool ValidateDirectory(string path)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine(Localization.Instance[AllTexts.MissingFiles]);
        string previewPath = GetPreviewPath(path);

        if (previewPath == null)
        {
            stringBuilder.AppendLine("preview.png");
        }

        var contentExist = true;

        foreach (string phrase in _requiredPhrasesData.RequiredPhrases)
        {
            bool isExist = File.Exists($"{path}/{phrase}.mp3");

            if (!isExist)
            {
                string file = Localization.Instance[AllTexts.File];
                string notFound = Localization.Instance[AllTexts.NotFound];
                Debug.LogError($"{file} {phrase}.mp3 {notFound} {path}");
                stringBuilder.AppendLine($"{phrase}.mp3");
                contentExist = false;
            }
        }
        
        if (previewPath == null || !contentExist)
        {
            _workshopError.SetErrorText(stringBuilder.ToString());
        }
        
        return previewPath != null && contentExist;
    }
    
    public static string GetPreviewPath(string path)
    {
        if(File.Exists($"{path}/preview.png"))
            return $"{path}/preview.png";
        if(File.Exists($"{path}/preview.jpg"))
            return $"{path}/preview.jpg";
        return File.Exists($"{path}/preview.gif") ? $"{path}/preview.gif" : null;
    }
}