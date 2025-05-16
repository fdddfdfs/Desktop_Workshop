using System;
using DG.Tweening;
using UnityEngine;

public abstract class AnimatedMenuWithView<TView> : MenuWithView<TView>  where TView: MonoBehaviour
{
    protected const float AnimationDuration = 1f;
    
    private bool _isActive;
    
    public override bool IsActive => _isActive;
    
    protected AnimatedMenuWithView(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
    }

    protected AnimatedMenuWithView(TView tView) : base(tView)
    {
    }

    public override void ChangeMenuActive(bool state, bool affectCursor)
    {
        _view.transform.DOKill(true);

        if (state)
        {
            _view.transform.localScale = Vector3.zero;
            _isActive = true;
            _view.transform.DOScale(1, AnimationDuration)
                .SetEase(Ease.OutBounce);
        }
        else
        {
            _isActive = false;
            _view.transform.localScale = Vector3.one;
            _view.transform.DOScale(0, AnimationDuration)
                .SetEase(Ease.OutBounce);
        }
    }
}