using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhrasesView : MonoBehaviour
{
    [SerializeField] private Button _close;
    [SerializeField] private GridLayoutGroup _phrasesParent;
    [SerializeField] private PhraseView _phrasePrefab;
    [SerializeField] private Sprite _morePhrasesSprite;
    
    private RectTransform _phrasesParentRectTransform;
    private RectTransform _phraseViewRectTransform;
    private PhraseView _morePhrases;
    
    public event Action<string> OnPhrasesSelected;
    public event Action<int> OnDefaultPhrasesSelected;
    public event Action OnClose;
    public event Action OnMorePhrasesSelected;
    
    private List<PhraseView> _phraseViews;

    public void AddPhrasesSet(Sprite sprite, int index)
    {
        PhraseView phraseView = AddPhrase(sprite);
        _morePhrases.transform.SetAsLastSibling();
        
        phraseView.OnPhraseSelect += () => OnDefaultPhrasesSelected?.Invoke(index);
    }
    
    public void AddPhrasesSet(Sprite sprite, string folder)
    {
        PhraseView phraseView = AddPhrase(sprite);
        _morePhrases.transform.SetAsLastSibling();
        
        phraseView.OnPhraseSelect += () => OnPhrasesSelected?.Invoke(folder);
    }

    private PhraseView AddPhrase(Sprite sprite)
    {
        _phrasesParentRectTransform.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            (_phraseViewRectTransform.rect.height +
             _phrasesParent.spacing.y) *
            Mathf.Floor((_phraseViews.Count + 1) / 
                        Mathf.Floor(_phraseViewRectTransform.rect.width / 
                            _phraseViewRectTransform.rect.width + _phrasesParent.spacing.x)) +
            _phrasesParent.padding.bottom +
            _phrasesParent.padding.top);
        
        GameObject phrase = Instantiate(_phrasePrefab.gameObject, _phrasesParent.transform);
        phrase.SetActive(true);
        var phraseView = phrase.GetComponent<PhraseView>();
        phraseView.UpdatePreview(sprite);
        _phraseViews.Add(phraseView);

        return phraseView;
    }

    private void Awake()
    {
        _close.onClick.AddListener(() => OnClose?.Invoke());
        
        _phrasesParentRectTransform = _phrasesParent.GetComponent<RectTransform>();
        _phraseViewRectTransform = _phrasePrefab.GetComponent<RectTransform>();
        
        _phraseViews = new List<PhraseView>();

        _morePhrases = AddPhrase(_morePhrasesSprite);
        _morePhrases.OnPhraseSelect += () => OnMorePhrasesSelected?.Invoke();
    }
}