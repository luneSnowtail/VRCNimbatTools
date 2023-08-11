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
        title = "Prefab Transform";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 65;
    }

    public override void CutieInspectorContent()
    {
        GUILayout.Label("Reset by property" , EditorStyles.miniLabel);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Position"))
        {
            NimbatFunctions.ResetPrefabPosition(activeTransform);
        }

        if (GUILayout.Button("Rotation"))
        {
            NimbatFunctions.ResetPrefabRotation(activeTransform);
        }

        if (GUILayout.Button("Scale"))
        {
            NimbatFunctions.ResetPrefabScale(activeTransform);
        }
        if (GUILayout.Button("All"))
        {
            NimbatFunctions.ResetPrefabPosition(activeTransform);
            NimbatFunctions.ResetPrefabRotation(activeTransform);
            NimbatFunctions.ResetPrefabScale(activeTransform);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();


        if (GUILayout.Button("Reset All Child Obects"))
        {
            NimbatFunctions.ResetPrefabPosition(activeTransform);
            NimbatFunctions.ResetPrefabRotation(activeTransform);
            NimbatFunctions.ResetPrefabScale(activeTransform);
        }

        GUILayout.EndHorizontal();
    }

    public override bool IsWindowValid()
    {
        if (Selection.activeGameObject)
        {
            return NimbatFunctions.IsPrefab(Selection.activeGameObject);
        }
        return false;
    }
}
