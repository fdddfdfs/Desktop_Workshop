using System;
using UnityEngine;

public class WorkshopSuccess : MenuWithView<WorkshopSuccessView>
{
    public WorkshopSuccess(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public WorkshopSuccess(WorkshopSuccessView tView) : base(tView)
    {
        _view.OnClose += () =>
        {
            ChangeMenuActive(false, false);
            Sounds.Instance.PlaySound(1, "Click");
        };
    }
}