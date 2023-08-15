using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class NimbatHandles : Editor
{
    static public bool DrawSphereButtonInCurve(float position, float radius, Color color)
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

    static public void DrawVRCNimbatObject(NimbatVRCObjectBase nimbatObject)
    {
        switch (nimbatObject.vrcObjectType)
        {
            case VRCObjectType.Collider:
                DrawAsCollider(nimbatObject);
                break;
            case VRCObjectType.Contact:
                DrawAsContact(nimbatObject);
                break;
            case VRCObjectType.PhysBone:
                DrawAsPhysbone(nimbatObject);
                break;
        }
    }

    static void DrawAsCollider(NimbatVRCObjectBase nimbatObject)
    {
        Color color = Handles.color;
        Handles.DrawDottedLine(nimbatObject.position, nimbatObject.positionFinal, 4f);

        if (nimbatObject.collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Sphere)
        {
            HandlesUtil.DrawWireSphere(nimbatObject.positionFinal, nimbatObject.vrcRadius_Scaled);
            Handles.color = new Color(0, 1, 0, .4f);
            HandlesUtil.DrawWireSphere(nimbatObject.positionFinal, nimbatObject.vrcRadius_Scaled - .01f);
            Handles.color = color;
        }
        else if (nimbatObject.collider.shapeType == VRCPhysBoneColliderBase.ShapeType.Capsule)
        {
            HandlesUtil.DrawWireCapsule(nimbatObject.positionFinal, nimbatObject.rotationFinal, (nimbatObject.collider.height) * nimbatObject.absoluteScale, nimbatObject.vrcRadius_Scaled);
            Handles.color = new Color(0, 1, 0, .4f);
            HandlesUtil.DrawWireCapsule(nimbatObject.positionFinal, nimbatObject.rotationFinal, (nimbatObject.collider.height * nimbatObject.absoluteScale - .003f), nimbatObject.vrcRadius_Scaled - .003f);
            Handles.color = color;
        }
    }

    static void DrawAsContact(NimbatVRCObjectBase nimbatObject)
    {
        Handles.DrawDottedLine(nimbatObject.position, nimbatObject.positionFinal, 4f);
        if (nimbatObject.contact.shapeType == ContactBase.ShapeType.Sphere)
        {
            HandlesUtil.DrawWireSphere(NimbatFunctions.GetContactPosition(nimbatObject.contact), nimbatObject.vrcRadius_Scaled);
        }
        else
        {
            HandlesUtil.DrawWireCapsule(nimbatObject.positionFinal, nimbatObject.rotationFinal, nimbatObject.contact.height * nimbatObject.absoluteScale, nimbatObject.vrcRadius_Scaled);
        }
    }

    static void DrawAsPhysbone(NimbatVRCObjectBase nimbatObject)
    {
        HandlesUtil.DrawWireSphere(nimbatObject.positionFinal, nimbatObject.vrcRadius_Scaled);

        if (nimbatObject.physBone.rootTransform)
        {
            Transform[] physboneChilds = nimbatObject.physBone.rootTransform.GetComponentsInChildren<Transform>();

            foreach (Transform child in physboneChilds)
            {
                if (child.childCount >= 1)
                    for (int i = 0; i < child.childCount; i++)
                    {
                        Handles.DrawLine(child.position, child.GetChild(i).position);
                    }
            }
        }
    }
}
