using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class NimbatHandles : Editor
{
    static public bool DrawSphereButton(float position, float radius, Color color)
    {
        Color handlesColor = Handles.color;

        Handles.color = color;

        if (Handles.Button(NimbatPhysBoneDrawer.GetPosition(position), Quaternion.identity, radius, radius, Handles.SphereHandleCap))
        {
            Handles.color = handlesColor;
            return true;
        }

        Handles.color = handlesColor;
        return false;
    }

}
