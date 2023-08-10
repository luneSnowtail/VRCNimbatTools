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

    static public bool DrawSphereButton(Vector3 position, float radius, Color color)
    {
        Color handlesColor = Handles.color;

        Handles.color = color;

        if (Handles.Button(position, Quaternion.identity, radius, radius, Handles.SphereHandleCap))
        {
            Handles.color = handlesColor;
            return true;
        }

        Handles.color = handlesColor;
        return false;
    }

    static public void DrawNimbatBone(NimbatBone bone, float radius)
    {
        if(bone.boneEnds.Count <= 0)
        {
            HandlesUtil.DrawWireSphere(bone.boneStart.position, radius);
            return;
        }

        HandlesUtil.DrawWireSphere(bone.boneStart.position, radius);

        for(int i = 0; i<bone.boneEnds.Count; i++)
        {
            DrawTriangle(bone.boneStart.position, bone.boneEnds[i].position, radius);
        }
    }

    static public void DrawTriangle(Vector3 start, Vector3 end, float radius)
    {
        Vector3 direction = start - end;

        Vector3 right = Vector3.right * radius;        

        Quaternion rotation =  Quaternion.FromToRotation(Vector3.forward, direction);

        Handles.DrawLine(start + (rotation * right), start - (rotation * right));
        Handles.DrawLine(start + (rotation * right),end);
        Handles.DrawLine(start - (rotation * right), end);

    }

}
