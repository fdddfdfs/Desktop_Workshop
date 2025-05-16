using System;
using System.Collections.Generic;
using UnityEngine;

public class Subtitles : MenuWithView<SubtitlesView>
{
    private readonly Dictionary<AudioClip, string> _subtitles;


    public Subtitles(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public Subtitles(SubtitlesView tView, List<SubtitlesData> data) : base(tView)
    {
        _subtitles = new Dictionary<AudioClip, string>();
        foreach (SubtitlesData item in data)
        {
            _subtitles[item.Clip] = item.Subtitle;
        }
        
        Sounds.Instance.OnSoundPlayed += SetSubtitle;
    }

    private void SetSubtitle(AudioClip clip)
    {
        if (_subtitles.TryGetValue(clip, out string subtitle))
        {
            _view.SetTextForTime(subtitle, clip.length);
        }
    }
}