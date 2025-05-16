using UnityEngine;

[CreateAssetMenu(fileName = "ModelData", menuName = "Data/ModelData")]
public class ModelData : ScriptableObject, IIcon
{
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Vector3 _hairsBoneGravity;
    [SerializeField] private AchievementData _achievementData;

    public GameObject Prefab => _prefab;

    public Sprite Icon => _icon;

    public Vector3 HairsBoneGravity => _hairsBoneGravity;

    public AchievementData AchievementData => _achievementData;
}