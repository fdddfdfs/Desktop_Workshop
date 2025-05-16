using UnityEngine;

public abstract class MenuWithView<TView> : MenuBase, IMenu  where TView: MonoBehaviour
{
    protected readonly TView _view;
    
    public override bool IsActive => _view.gameObject.activeSelf;

    protected MenuWithView(Transform parent, string menuViewResourceName)
    {
        _view = ResourcesLoader.InstantiateLoadComponent<TView>(menuViewResourceName);
        _view.transform.SetParent(parent, false);
    }

    protected MenuWithView(TView tView)
    {
        _view = tView;
    }

    public override void SetAsLastSibling()
    {
        _view.transform.SetAsLastSibling();
    }

    public override void ChangeMenuActive(bool state, bool affectCursor)
    {
        base.ChangeMenuActive(state, affectCursor);
        
        _view.gameObject.SetActive(state);
    }

    public virtual void ChangeMenuActive()
    {
        _view.gameObject.SetActive(!_view.gameObject.activeSelf);
    }
}