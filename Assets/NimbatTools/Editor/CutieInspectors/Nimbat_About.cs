using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Nimbat_About : NimbatCutieInspectorWindow
{
    public Nimbat_About()
    {
        title = "About";
        width = NimbatData.inspectorWindowsWidth;
        height = 200;

        mainButtonIconPath = "Button_about";
    }

    public override void CutieInspectorContent()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("Special thanks to the cute beans:");

        GUILayout.Label("Hikari for being a wonderful cutie" , EditorStyles.wordWrappedMiniLabel);
        GUILayout.Label("Twikle_Sparlight for their awesome friendship and support", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Label("Sylv for doing the nimbat >=(", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Label("Masho for all their crimes on twitch D=", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Label("Kitsunai for releasing all the awoos!!", EditorStyles.wordWrappedMiniLabel);

        GUILayout.EndVertical();
    }

}
