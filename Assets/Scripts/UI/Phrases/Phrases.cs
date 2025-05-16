using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

public class Phrases : AnimatedMenuWithView<PhrasesView>
{
    private readonly RequiredPhrasesData _requiredPhrasesData;
    private readonly List<PhrasesData> _phrasesData;

    public Phrases(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public Phrases(
        PhrasesView tView,
        RequiredPhrasesData requiredPhrasesData,
        List<PhrasesData> phrasesData) 
        : base(tView)
    {
        _requiredPhrasesData = requiredPhrasesData;
        _phrasesData = phrasesData;

        _view.OnPhrasesSelected += (folder) =>
        {
            Coroutines.StartRoutine(ChangePhrases(folder));
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _view.OnDefaultPhrasesSelected += (index) =>
        {
            ChangeToDefaultPhrases(index);
            Sounds.Instance.PlaySound(1, "Click");
        };
        
        _view.OnClose += () =>
        {
            ChangeMenuActive(!IsActive, false);
            Sounds.Instance.PlaySound(1, "Click");
            Achievements.Instance.GetAchievement("CloseMenu");
        };

        _view.OnMorePhrasesSelected += () =>
        {
            ShowMorePhrases();
            Achievements.Instance.GetAchievement("MoreVoicePacks");
        };
        
        _view.transform.localScale = Vector3.zero;
        
        for (int i = 0; i < _phrasesData.Count; i++)
        {
            _view.AddPhrasesSet(_phrasesData[i].Icon, i);
        }
    }

    public IEnumerator AddNewPhrasesSet(string folder)
    {
        string previewPath = Workshop.GetPreviewPath(folder);
        using UnityWebRequest previewRequest = UnityWebRequestTexture.GetTexture(previewPath);
        yield return previewRequest.SendWebRequest();

        Texture2D previewTexture = null;

        if (previewRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error downloading preview image: {previewRequest.error}");
        }
        else
        {
            previewTexture = DownloadHandlerTexture.GetContent(previewRequest);

            Debug.Log($"Preview image loaded successfully from {previewPath}");
        }
        
        if (previewTexture != null)
        {
            var previewSprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0, 0));
            
            _view.AddPhrasesSet(previewSprite, folder);
        }
    }

    private void ChangeToDefaultPhrases(int index)
    {
        Sounds.Instance.ReplaceSounds(_phrasesData[index].AudioClips);
    }

    private IEnumerator ChangePhrases(string folder)
    {
        List<AudioClip> newPhrasesSet = new();

        foreach (string clip in _requiredPhrasesData.RequiredPhrases)
        {
            string uri = new Uri($"{folder}/{clip}.mp3").AbsoluteUri;

            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error downloading audio clip: {request.error}");
            }
            else
            {
                AudioClip clipLoaded = DownloadHandlerAudioClip.GetContent(request);
                clipLoaded.name = clip;
                newPhrasesSet.Add(clipLoaded);

                Debug.Log($"Audio clip {clip} loaded successfully from {uri}");
            }
        }
        
        Sounds.Instance.ReplaceSounds(newPhrasesSet);
    }

    private static void ShowMorePhrases()
    {
        if (!SteamManager.Initialized) return;
        
        AppId_t appId = SteamUtils.GetAppID();
        
        SteamFriends.ActivateGameOverlayToWebPage(
            $"https://steamcommunity.com/workshop/browse/?appid={appId.m_AppId}&browsesort=trend&section=readytouseitems");
    }
}