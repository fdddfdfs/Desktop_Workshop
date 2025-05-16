using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SubtitlesData", menuName = "Data/SubtitlesData")]
public class SubtitlesData : ScriptableObject
{
    [SerializeField] private AudioClip _clip;
    [SerializeField] private string _subtitle;
    
    public AudioClip Clip => _clip;
    public string Subtitle => _subtitle;
}