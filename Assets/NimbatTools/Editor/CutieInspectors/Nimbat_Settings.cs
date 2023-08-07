using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nimbat_Settings : NimbatCutieInspectorWindow
{
    public Nimbat_Settings()
    {
        title = "Settings";
        width = NimbatData.inspectorWindowsWidth;
        height = 150;
        mainButtonIconPath = "Button_settings";
    }

    static public bool enableMirrorMode = true;
    static public bool holdControlToSelect = true;

    public override void CutieInspectorContent()
    {
        enableMirrorMode = GUILayout.Toggle(enableMirrorMode, "Highlight mirror object on selection");
        holdControlToSelect = GUILayout.Toggle(holdControlToSelect, "Hold Ctrl to select vrc Object");
    }
}
