using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Sounds : ResourcesSingleton<Sounds, SoundsResourceName>
{
    [SerializeField] private List<AudioSource> _soundsSources;
    [SerializeField] private List<AudioClip> _soundsClips;
    [SerializeField] private List<SoundData> _soundsData;

    private Dictionary<string, AudioClip> _clips;
    private Dictionary<string, int> _randomClipsCount;
    private Dictionary<string, float> _soundsVolume;

    private float _currentVolume = 1;

    public event Action<AudioClip> OnSoundPlayed;

    public void ChangeSoundsVolume(float newVolume)
    {
        _currentVolume = newVolume;
    }
    
    public async void DisableAudioForTime(int layer, float time)
    {
        CancellationToken token = AsyncUtils.Instance.GetCancellationToken();
        _soundsSources[layer].enabled = false;
        
        await AsyncUtils.Instance.Wait(time, token);

        if (token.IsCancellationRequested) return;
        
        _soundsSources[layer].clip = null;
        _soundsSources[layer].enabled = true;
    }

    public void PlaySound(int layer, string clipName)
    {
        AudioSource source = _soundsSources[layer];
        
        if (!source.enabled) return;
        
        CheckLayer(layer);

        clipName = clipName.ToLower();

        if (!_clips.ContainsKey(clipName))
        {
            throw new Exception($"Clip with name: {clipName} doesnt exist in {nameof(Sounds)}");
        }

        source.volume = _soundsVolume.GetValueOrDefault(clipName, 1) * _currentVolume;
        
        source.clip = _clips[clipName];
        source.Play();
        
        OnSoundPlayed?.Invoke(source.clip);
    }

    public void PlayRandomSounds(int layer, string clipBaseName)
    {
        AudioSource source = _soundsSources[layer];
        
        if (!source.enabled) return;
        
        CheckLayer(layer);
        
        clipBaseName = clipBaseName.ToLower();
        
        if (!_randomClipsCount.TryGetValue(clipBaseName, out int clipCount))
        {
            throw new Exception($"{clipBaseName} doesnt exist in random sounds in {nameof(Sounds)}");
        }
        
        int r = Random.Range(1, clipCount + 1);

        string clipName = clipBaseName + r;

        if (!_clips.ContainsKey(clipName))
        {
            throw new Exception(
                $"Clip with name: {clipName} doesnt exist in {nameof(Sounds)}," +
                $" clips with {clipBaseName} probably have missing numbers");
        }

        source.volume = _soundsVolume.GetValueOrDefault(clipName, 1) * _currentVolume;
        source.clip = _clips[clipName];
        source.Play();
        
        OnSoundPlayed?.Invoke(source.clip);
    }

    public void ReplaceSounds(List<AudioClip> sounds)
    {
        foreach (AudioClip audioClip in sounds)
        {
            ReplaceSound(audioClip);
        }
    }

    public void ReplaceSound(AudioClip sound)
    {
        _clips[sound.name.ToLower()] = sound;
    }

    public void StopSound(int layer)
    {
        CheckLayer(layer);

        _soundsSources[layer].Stop();
    }

    public void StopAllSounds()
    {
        foreach (AudioSource soundsSource in _soundsSources)
        {
            soundsSource.Stop();
        }
    }

    private void Awake()
    {
        _soundsSources ??= GetComponents<AudioSource>().ToList();
        
        _randomClipsCount = new Dictionary<string, int>();
        _clips = new Dictionary<string, AudioClip>();
        foreach (AudioClip soundsClip in _soundsClips)
        {
            string clipName = soundsClip.name.ToLower();
            
            if (char.IsDigit(clipName[^1]))
            {
                int index = clipName.Length - 1;
                while (char.IsDigit(clipName[index - 1]))
                {
                    index--;
                }

                string baseName = clipName[..index];

                if (!_randomClipsCount.TryAdd(baseName, 1))
                {
                    _randomClipsCount[baseName] += 1;
                }
            }
            
            _clips.Add(clipName, soundsClip);
        }

        _soundsVolume = new Dictionary<string, float>();
        for (var i = 0; i < _soundsData.Count; i++)
        {
            _soundsVolume[_soundsData[i].AudioClip.name.ToLower()] = _soundsData[i].SoundVolume;
        }
    }

    private void CheckLayer(int layer)
    {
        if (layer > _soundsSources.Count)
        {
            throw new IndexOutOfRangeException($"{layer} more then sounds sources count: {_soundsSources.Count}");
        }
    }
}

public class SoundsResourceName : ResourceName
{
    public override string Name => "Audio/Sounds";
}