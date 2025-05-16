using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelClothCustomization
{
    [SerializeField] private SkinnedMeshRenderer _cloth;
    [SerializeField] private List<Material> _customization;

    public SkinnedMeshRenderer Cloth => _cloth;
    public List<Material> Customization => _customization;
}