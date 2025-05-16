using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelView : MonoBehaviour
{
    private const string ResetTriggerName = "Reset";
    private const string SpeedMultiplierName = "Speed";
    
    [SerializeField] private GameObject _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private BoxCollider _boxCollider;
    [SerializeField] private List<CustomizationMesh> _customizationMeshes;
    [SerializeField] private DynamicBone _hairsBone;
    [SerializeField] private float _bodyWeight;
    [SerializeField] private float _headWeight;
    [SerializeField] private List<ModelClothesSet> _clothes;
    [SerializeField] private SkinnedMeshRenderer _head;

    private readonly int _resetTriggerHash = Animator.StringToHash(ResetTriggerName);
    private readonly int _speedMultiplierHash = Animator.StringToHash(SpeedMultiplierName);

    private AnimatorOverrideController _animatorOverrideController;
    private string _defaultClipName;
    private int _defaultStateName;

    private Camera _mainCamera;

    private Dictionary<CustomizationType, List<CustomizationMesh>> _customizationMeshesByType;

    private int _currentClothes;

    public Vector2 Size => _boxCollider.size * (Vector2)_boxCollider.transform.localScale;
    public Transform ModelTransform => _model.transform;
    public BoxCollider BoxCollider => _boxCollider;
    public Animator Animator => _animator;

    public float GetCurrentAnimationNormalizedTime()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void MoveHeadBlendShapes(bool isReversed)
    {
        int direction = isReversed ? 1 : 0;
        
        AsyncUtils.Instance.Wait(
            1f,
            (progress) =>
            {
                progress = Mathf.Abs(direction - progress) * 100;
                _head.SetBlendShapeWeight(5, progress);
                _head.SetBlendShapeWeight(11, progress);
                _head.SetBlendShapeWeight(49, progress);
                _head.SetBlendShapeWeight(51, progress);
            });
    }

    public void ChangeActive(bool active)
    {
        _model.SetActive(active);
    }

    public void ChangeAnimation(AnimationClip animationClip, float normalizedTime = 0)
    {
        _animatorOverrideController[_defaultClipName] = animationClip;
        if (normalizedTime == 0)
        {
            _animator.SetTrigger(_resetTriggerHash);
        }
        else
        {
            _animator.Play(_defaultStateName, 0, normalizedTime);
        }
    }

    public void Customize(CustomizationType type, Material newMaterial)
    {
        foreach (CustomizationMesh customizationMesh in _customizationMeshesByType[type])
        {
            Material[] materials = customizationMesh.MeshRenderer.materials;
            materials[customizationMesh.Index] = newMaterial;
            customizationMesh.MeshRenderer.materials = materials;
        }
    }

    public void CustomizeHairs(HairData hairData)
    {
        foreach (HairMaterial hairMaterial in hairData.HairMaterials)
        {
            if (!_customizationMeshesByType.TryGetValue(
                    hairMaterial.CustomizationType,
                    out List<CustomizationMesh> value))
            {
                continue;
            }
            
            foreach (CustomizationMesh customizationMesh in value)
            {
                Material[] materials = customizationMesh.MeshRenderer.materials;
                materials[customizationMesh.Index] = hairMaterial.HairsMaterial;
                customizationMesh.MeshRenderer.materials = materials;
            }
        }
    }

    public void CustomizeWeight(CustomizationType type, float newWeight)
    {
        foreach (CustomizationMesh customizationMesh in _customizationMeshesByType[type])
        {
            if (type == CustomizationType.TopBody)
            {
                if (newWeight > 0.5f)
                {
                    customizationMesh.MeshRenderer.SetBlendShapeWeight(
                        customizationMesh.Index,
                        (newWeight * 2 - 1) * 100);
                    customizationMesh.MeshRenderer.SetBlendShapeWeight(
                        customizationMesh.Index + 1, 0);
                }
                else
                {
                    customizationMesh.MeshRenderer.SetBlendShapeWeight(customizationMesh.Index, 0);
                    customizationMesh.MeshRenderer.SetBlendShapeWeight(
                        customizationMesh.Index + 1, 
                        (1 - newWeight * 2) * 100);
                }
            }
            else
            {
                customizationMesh.MeshRenderer.SetBlendShapeWeight(customizationMesh.Index, newWeight * 100);
            }
        }
    }

    public void ChangeHairsBoneGravity(Vector3 newGravity)
    {
        if (!_hairsBone) return;
        
        _hairsBone.m_Gravity = newGravity;
    }

    public void ChangeAnimationSpeed(float speed)
    {
        _animator.SetFloat(_speedMultiplierHash, speed);
    }

    public void Init(Camera mainCamera)
    {
        _mainCamera = mainCamera;
        var modelViewHeadController = _model.AddComponent<ModelViewHeadController>();
        modelViewHeadController.Init(_mainCamera, _animator, _bodyWeight, _headWeight);
    }

    public void ChangeClothes(int index = -1)
    {
        foreach (ModelClothCustomization modelClothCustomization in _clothes[_currentClothes].ClothCustomization)
        {
            modelClothCustomization.Cloth.gameObject.SetActive(false);
        }

        if (index == -1) index = UnityEngine.Random.Range(0, _clothes.Count - 1);
        
        foreach (ModelClothCustomization modelClothCustomization in _clothes[index].ClothCustomization)
        {
            modelClothCustomization.Cloth.gameObject.SetActive(true);
            int materialIndex = UnityEngine.Random.Range(0, modelClothCustomization.Customization.Count);
            modelClothCustomization.Cloth.material = modelClothCustomization.Customization[materialIndex];
        }

        _currentClothes = index;
    }

    private void Awake()
    {
        foreach (ModelClothesSet modelClothesSet in _clothes)
        {
            foreach (ModelClothCustomization modelClothCustomization in modelClothesSet.ClothCustomization)
            {
                modelClothCustomization.Cloth.gameObject.SetActive(false);
            }
        }
        
        return;
        _customizationMeshesByType = new Dictionary<CustomizationType, List<CustomizationMesh>>();
        foreach (CustomizationMesh customizationMesh in _customizationMeshes)
        {
            if (!_customizationMeshesByType.ContainsKey(customizationMesh.CustomizationType))
            {
                _customizationMeshesByType[customizationMesh.CustomizationType] = new List<CustomizationMesh>();
            }
            
            _customizationMeshesByType[customizationMesh.CustomizationType].Add(customizationMesh);
        }

        _animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _animatorOverrideController;
        _defaultClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        _defaultStateName = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        _animator.applyRootMotion = false;
        _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }
}