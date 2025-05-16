using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomizationData", menuName = "Data/CustomizationData")]
public class CustomizationData : ScriptableObject, IIcon
{
    [SerializeField] private Material _material;
    [SerializeField] private Sprite _icon;
    [SerializeField] private CustomizationType _customizationType;
    [SerializeField] private List<ModelData> _eligibleModels;

    public CustomizationType CustomizationType => _customizationType;
    
    public Material Material => _material;

    public Sprite Icon => _icon;

    public List<ModelData> EligibleModels => _eligibleModels;
}