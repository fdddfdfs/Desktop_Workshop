using UnityEngine;

public abstract class MenuBase
{
    public abstract bool IsActive { get; }
    
    public virtual void ChangeMenuActive(bool state, bool affectCursor)
    {
        if (IsActive == state) return;

        if (!affectCursor) return;
        
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

    public abstract void SetAsLastSibling();

    public virtual bool SwapActive(bool affectCursor = true)
    {
        ChangeMenuActive(!IsActive, affectCursor);

        return IsActive;
    }

    public void ChangeCursorActive(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
}