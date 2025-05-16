using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Achievements/AchievementData")]
public sealed class AchievementData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _description;
    [SerializeField] private int _requiredProgress;
    [SerializeField] private bool _isLifetime;

    public string Name => _name;
    public Sprite Sprite => _sprite;
    public int RequiredProgress => _requiredProgress;
    
    public string Description => _description;

    public bool IsLifetime => _isLifetime;
}