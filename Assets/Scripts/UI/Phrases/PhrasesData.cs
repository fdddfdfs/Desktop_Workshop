using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhrasesData", menuName = "Data/PhrasesData")]
public class PhrasesData : ScriptableObject
{
    [SerializeField] private List<AudioClip> _audioClips;
    [SerializeField] private Sprite _icon;
    
    public List<AudioClip> AudioClips => _audioClips;
    public Sprite Icon => _icon;
}