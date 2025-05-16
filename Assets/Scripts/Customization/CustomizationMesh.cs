using System;
using UnityEngine;

[Serializable]
public class CustomizationMesh
{
    [SerializeField] private CustomizationType _customizationType;
    [SerializeField] private SkinnedMeshRenderer _meshRenderer;
    [SerializeField] private int _index;

    public CustomizationType CustomizationType => _customizationType;
    public SkinnedMeshRenderer MeshRenderer => _meshRenderer;
    public int Index => _index;
}