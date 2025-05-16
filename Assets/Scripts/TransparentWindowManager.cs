using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TransparentWindowManager : SingletonMonoBehaviour<TransparentWindowManager>
{
    [SerializeField] private Camera _camera;
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private GraphicRaycaster _graphicRaycaster;
    [SerializeField] private InputActionAsset _input;
    [SerializeField] private EntryPoint _entryPoint;
    
    private List<RaycastResult> _uiRaycastResults;
    private PointerEventData _pointerEventData;
    private bool _isActivatingWindow;
    
    private bool _prevClickThrough;

    private int _ignoreUILayer;
    
    const int GWL_STYLE = -16;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const int HWND_TOPMOST = -1;
    IntPtr hwnd;
    int fWidth;
    int fHeight;
    MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
    #region Enum

    internal enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_INVALID_STATE = 4
    }

    #endregion Enum

    #region Struct

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    internal struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    #endregion Struct

    #region DLL Import

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hwnd);

    #endregion DLL Import

    #region Method

    // CAUTION:
    // To control enable or disable, use Start method instead of Awake.
    
    private void Start()
    {
        _ignoreUILayer = LayerMask.NameToLayer("IgnoreUI");
        // #if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        hwnd = GetActiveWindow();
        // fWidth = Screen.width;
        //fHeight = Screen.height;
        fWidth = Screen.currentResolution.width;
        fHeight = Screen.currentResolution.height;
        const int GWL_STYLE = -16;
        const uint WS_POPUP = 0x80000000;
        const uint WS_VISIBLE = 0x10000000;

        // NOTE:
        // https://msdn.microsoft.com/ja-jp/library/cc410861.aspx

        var windowHandle = GetActiveWindow();

        // NOTE:
        // https://msdn.microsoft.com/ja-jp/library/cc411203.aspx
        // 
        // "SetWindowLong" is used to update window parameter.
        // The arguments shows (target, parameter, value).

        SetWindowLong(windowHandle, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // NOTE:
        // https://msdn.microsoft.com/ja-jp/library/windows/desktop/aa969512(v=vs.85).aspx
        // https://msdn.microsoft.com/ja-jp/library/windows/desktop/bb773244(v=vs.85).aspx
        // 
        // DwmExtendFrameIntoClientArea will spread the effects
        // which attached to window frame to contents area.
        // So if the frame is transparent, the contents area gets also transparent.
        // MARGINS is structure to set the spread range.
        // When set -1 to MARGIN, it means spread range is all of the contents area.

        MARGINS margins = new MARGINS()
        {
            cxLeftWidth = -1
        };

        DwmExtendFrameIntoClientArea(windowHandle, ref margins);

        //#endif // !UNITY_EDITOR && UNITY_STANDALONE_WIN

        _uiRaycastResults = new List<RaycastResult>();
        _pointerEventData = new PointerEventData(_eventSystem);
    }

    private void Update()
    {
        bool clickThrough = !Physics.Raycast(
            _camera.ScreenPointToRay(Input.mousePosition).origin,
            _camera.ScreenPointToRay(Input.mousePosition).direction,
            out _,
            1000,
            Physics.DefaultRaycastLayers);

        if (clickThrough)
        {
            _pointerEventData.position = Input.mousePosition;
            _uiRaycastResults.Clear();
            _graphicRaycaster.Raycast(_pointerEventData, _uiRaycastResults);

            foreach (RaycastResult raycastResult in _uiRaycastResults)
            {
                if (raycastResult.gameObject.layer != _ignoreUILayer)
                {
                    clickThrough = false;
                    break;
                }
            }

            if (clickThrough)
            {
                GraphicRaycaster additionalRaycaster = _entryPoint.GraphicRaycaster;

                if (additionalRaycaster)
                {
                    _uiRaycastResults.Clear();
                    additionalRaycaster.Raycast(_pointerEventData, _uiRaycastResults);

                    if (_uiRaycastResults.Count != 0)
                    {
                        clickThrough = false;
                    }
                }
            }
        }

        BringWindowToTop(hwnd);
        
        if (clickThrough != _prevClickThrough)
        {
            if (clickThrough)
            {
//#if !UNITY_EDITOR
                SetWindowLong(hwnd, -20, (uint)524288 | (uint)32| (uint)8);
            SetLayeredWindowAttributes (hwnd, 0, 255, 2);// Transparency=51=20%, LWA_ALPHA=2
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0,fWidth, fHeight, 0x0002 | 0x0001);
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, fWidth, fHeight, 32 | 64); //SWP_FRAMECHANGED = 0x0020 (32); //SWP_SHOWWINDOW = 0x0040 (64)
            //other code
//#endif
            }
            else
            {
//#if !UNITY_EDITOR
                SetWindowLong(hwnd, -20, WS_POPUP | WS_VISIBLE);
            SetLayeredWindowAttributes (hwnd, 0, 255, 2);
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, fWidth, fHeight, 16 | 32 ); //SWP_FRAMECHANGED = 0x0020 (32); //SWP_SHOWWINDOW = 0x0040 (64)
//#endif
            }
            _prevClickThrough = clickThrough;
        }
    }


    #endregion Method
}











