using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HairData", menuName = "Data/HairData")]
public class HairData : CustomizationData
{
    [SerializeField] private List<HairMaterial> _hairMaterials;

    public List<HairMaterial> HairMaterials => _hairMaterials;
}

[Serializable]
public class HairMaterial
{
    [SerializeField] private CustomizationType _customizationType;
    [SerializeField] private Material _hairsMaterial;

    public CustomizationType CustomizationType => _customizationType;
    public Material HairsMaterial => _hairsMaterial;
}