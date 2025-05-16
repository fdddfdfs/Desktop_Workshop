using System;
using UnityEngine;
using UnityEngine.UI;

public class PhraseView : MonoBehaviour
{
    [SerializeField] private Image _preview;
    [SerializeField] private Button _selectButton;

    public event Action OnPhraseSelect;
    
    public void UpdatePreview(Sprite sprite)
    {
        _preview.sprite = sprite;
        Vector3 localScale = _preview.transform.localScale;
        float heightScale = sprite.rect.height / sprite.rect.width;
        _preview.transform.localScale = new Vector3(localScale.x, localScale.y * heightScale, localScale.z);
    }

    private void Awake()
    {
        _selectButton.onClick.AddListener(() => OnPhraseSelect?.Invoke());
    }
}