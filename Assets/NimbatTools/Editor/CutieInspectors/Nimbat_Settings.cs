using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Nimbat_Settings : NimbatCutieInspectorWindow
{
    public Nimbat_Settings()
    {
        title = "Settings";
        width = NimbatData.inspectorWindowsWidth;
        height = 70;
        mainButtonIconPath = "Button_settings";
    }

    static public bool enableMirrorMode = true;
    static public bool holdControlToSelect = true;

    public override void CutieInspectorContent()
    {
        GUILayout.Label("Alpha version");
        GUILayout.Label("settings have been moved to their own place in the cutie inspector windows in the bottom right when an object is selected", EditorStyles.wordWrappedMiniLabel); 

        /*
        enableMirrorMode = GUILayout.Toggle(enableMirrorMode, "Highlight mirror object on selection");
        holdControlToSelect = GUILayout.Toggle(holdControlToSelect, "Hold Ctrl to select vrc Object");*/
    }
}
