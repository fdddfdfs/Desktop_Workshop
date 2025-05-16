using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModelClothesSet
{
    [SerializeField] private List<ModelClothCustomization> _clothCustomization;

    public List<ModelClothCustomization> ClothCustomization => _clothCustomization;
}