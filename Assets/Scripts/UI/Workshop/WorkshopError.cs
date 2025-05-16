using System;
using UnityEngine;

public class WorkshopError : MenuWithView<WorkshopErrorView>
{
    public WorkshopError(Transform parent, string menuViewResourceName) : base(parent, menuViewResourceName)
    {
        throw new NotImplementedException();
    }

    public WorkshopError(WorkshopErrorView tView) : base(tView)
    {
        _view.OnClose += ChangeMenuActive;
        
        ChangeMenuActive(false, false);
    }
    
    public void SetErrorText(string errorText)
    {
        ChangeMenuActive(true, false);
        _view.UpdateErrorText(errorText);
    }
}