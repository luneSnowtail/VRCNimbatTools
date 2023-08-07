using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Nimbat_AvatarSettings : NimbatCutieInspectorWindow
{
   
    public Nimbat_AvatarSettings()
    {
        title = "Avatar Settings";
        width = NimbatData.inspectorWindowsWidth;
        height = 100;

        mainButtonIconPath = "Button_avatar";
    }


    public override void CutieInspectorContent()
    {
        GUILayout.Label("Feature still in progress, not ready for beta", EditorStyles.wordWrappedLabel);
    }
}
