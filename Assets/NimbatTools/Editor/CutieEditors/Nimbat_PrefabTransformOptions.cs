using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// This class is in charge of resetig a gameobject marked as a prefab to its default state
/// only transform stuff
/// </summary>
public class Nimbat_PrefabTransformOptions : NimbatCutieInspectorWindow
{
    PrefabInstanceStatus prefabStatus;
    Transform _activeTransform;
    public Transform activeTransform
    {
        get
        {
            return _activeTransform;
        }
        set
        {
            if (value)
            {
                isEnabled = NimbatFunctions.IsPrefab(value.gameObject);
            }
            else
            {
                isEnabled = false;
            }

            _activeTransform = value;
        }
    }
    

    public Nimbat_PrefabTransformOptions()
    {
        title = "Transform Prefab data";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 65;
    }

    public override void CutieInspectorContent()
    {
        GUILayout.Label("Reset by property");
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Position", EditorStyles.toolbarButton))
        {
            NimbatFunctions.ResetPrefabPosition(activeTransform);
        }

        if (GUILayout.Button("Rotation", EditorStyles.toolbarButton)) 
        {
            NimbatFunctions.ResetPrefabRotation(activeTransform);
        }

        if (GUILayout.Button("Scale", EditorStyles.toolbarButton))
        {
            NimbatFunctions.ResetPrefabScale(activeTransform);
        }

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset all", EditorStyles.toolbarButton))
        {
            NimbatFunctions.ResetPrefabPosition(activeTransform);
            NimbatFunctions.ResetPrefabRotation(activeTransform);
            NimbatFunctions.ResetPrefabScale(activeTransform);
        }
    }


}
