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
        height = 310;

        mainButtonIconPath = "Button_about";
    }

    public override void CutieInspectorContent()
    {

        GUILayout.Label("Nimbat tools Alpha v0.5");
        GUILayout.Label("I still expect this tool to have many bugs but please help me report them at discord or git website, thanks for helping me make this tool better",EditorStyles.wordWrappedMiniLabel);

        GUILayout.Label("Even when this tool is and will always be free, you can still support me at patreon", EditorStyles.wordWrappedLabel);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Patreon"))
        {
            Application.OpenURL("https://patreon.com/nimbattools");
        }
        if (GUILayout.Button("Git"))
        {
            Application.OpenURL("https://github.com/luneSnowtail/VRCNimbatTools");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Bsky"))
        {
            Application.OpenURL("https://bsky.app/profile/nimbat.live");
        }
        if (GUILayout.Button("Twitch"))
        {
            Application.OpenURL("https://twitch.tv/lunesnowtail");
        }
        if (GUILayout.Button("Linktree"))
        {
            Application.OpenURL("https://linktr.ee/lunesnowtail");
        }
        if (GUILayout.Button("Twitter"))
        {
            Application.OpenURL("https://twitter.com/lunesnowtail");
        }


        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();        
        GUILayout.Label("");

        GUILayout.Label("Special thanks to the cute beans:");

        string seanNia = "Sean and Nia for their love and for being special ";
        string hikari = "Hikari, for their awesome support ";
        string twinkle = "Twikle_Sparlight for their friendship and support ";
        string masho = "Masho for all their crimes on twitch D= ";
        string sylv = "Sylv for doing the nimbat >=( ";
        string kitsunai = "Kitsunai for releasing all the awoos!! and their help with testing ";
        string dan = "Dan for their dino advices and for surviving the hurriquake ";
        string nul = "Nul for the code review and advices ";
        string apois = "Apois for hardcore testing ";

        string composedString = string.Concat(seanNia, hikari, twinkle, masho, kitsunai, dan, nul, apois, sylv);

        GUILayout.Label(composedString, EditorStyles.wordWrappedMiniLabel);

        GUILayout.EndVertical();
    }

}
