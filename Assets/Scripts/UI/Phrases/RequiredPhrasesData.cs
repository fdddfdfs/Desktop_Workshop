using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RequiredPhrasesData", menuName = "Data/RequiredPhrasesData")]
public class RequiredPhrasesData : ScriptableObject
{
    [SerializeField] private List<string> _requiredPhrases;
    
    public List<string> RequiredPhrases => _requiredPhrases;
}