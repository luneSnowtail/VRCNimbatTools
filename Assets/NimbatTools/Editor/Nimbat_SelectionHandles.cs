using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

/// <summary>
/// This class will be deleted soon
/// </summary>

public class Nimbat_SelectionHandles : EditorWindow
{
    static public SelectedVRCObjectData selectedVRCObject;
    static public SelectedVRCObjectData mirroredVRCObject;

    static ContactBase tempContact;
    static VRCPhysBoneBase tempPhysbone;
    static GameObject mirrorGameObject;

    static bool mirrorObject;
    static bool mirrorPosition;
    static bool mirrorRadius;

    static Color leftSideColor = new Color(0, 0, 1, .3f);
    static Color rightSideColor = new Color(1, 0, 0, .3f);
    static Color handlesDefaultColor;

    static VRCPhysBoneBase activePhysbone;
    static Transform rootTransform;

    static int selectedCurveKey = 0;
    static float outTanValue;
    static float outHandleAngle;
    static Keyframe keyframeData;

    static Vector3 testAngle;

    static public void RemoveSelectedObject()
    {
        selectedVRCObject.vrcObject.contact = null;
        selectedVRCObject.vrcObject.physBone = null;        
    }

    static public void SetSelectedObject(GameObject selectedGameObject)
    {

        tempPhysbone = selectedGameObject.GetComponent<VRCPhysBoneBase>();
        if (tempPhysbone)
        {
            selectedVRCObject.vrcObject.physBone = tempPhysbone;
            activePhysbone = selectedVRCObject.vrcObject.physBone;

            if (activePhysbone.rootTransform.childCount > 1)
            {
                rootTransform = activePhysbone.rootTransform.GetChild(0);
            }
            else
            {
                rootTransform = activePhysbone.rootTransform;
            }

            NimbatPhysBoneDrawer.UpdateChain(rootTransform);

            selectedCurveKey = 0;

            return;
        }


        tempContact = selectedGameObject.GetComponent<ContactBase>();
        if (tempContact)
        {
            selectedVRCObject.vrcObject.contact = tempContact;

            mirrorGameObject = NimbatCore.vrcMirrorGroups.GetMirrorFromGameObject(selectedGameObject);

            if (!mirrorGameObject)
                return;

            tempContact = mirrorGameObject.GetComponent<ContactBase>();

            if (tempContact)
            {
                mirroredVRCObject.vrcObject.contact = tempContact;
                mirrorObject = true;
            }
            else
            {
                mirrorObject = false;
            }
            return;
        }

        selectedVRCObject.vrcObject.physBone = null;
        selectedVRCObject.vrcObject.contact = null;
        tempContact = null;
        tempPhysbone = null;
    }

    static public void DrawObjectEditHandle()
    {
        handlesDefaultColor = Handles.color;

        switch (selectedVRCObject.vrcObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                DrawSelectedContactHandle();
                break;
            case VRCObjectType.PhysBone:
                DrawSelectedPhysboneHandles();
                break;
        }
    }

    static public void DrawObjectEditOptions()
    {
        if(selectedVRCObject.vrcObject.vrcObjectType == VRCObjectType.None)
        {    
            return;
        }

        GUILayout.BeginVertical();

        switch (selectedVRCObject.vrcObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                mirrorPosition = GUILayout.Toggle(mirrorPosition, "realtime position mirror");
                mirrorRadius = GUILayout.Toggle(mirrorRadius, "realtime radius mirror");
                
                break;
            case VRCObjectType.PhysBone:

                if (activePhysbone)
                {
                    GUILayout.Label(activePhysbone.name);

                    if(activePhysbone.rootTransform)
                        GUILayout.Label(activePhysbone.rootTransform.name);

                    GUILayout.Label(NimbatPhysBoneDrawer.physBoneTransform.Count.ToString());

                    GUILayout.Label(keyframeData.outTangent.ToString());
                    GUILayout.Label(keyframeData.outWeight.ToString());
                    GUILayout.Label(outHandleAngle.ToString());
                }

                break;
        }


        GUILayout.EndVertical();
    }

    /// <summary>
    /// If the object we have selected is a contact, we draw the handles to edit it
    /// </summary>
    static public void DrawSelectedContactHandle()
    {        
        Handles.Label(Vector3.zero, "");

        ContactBase activeContact;
        activeContact = selectedVRCObject.vrcObject.contact;
        ContactBase mirrorContact;
        mirrorContact = mirroredVRCObject.vrcObject.contact;

        
        Handles.Label(NimbatFunctions.GetContactPosition(activeContact), activeContact.transform.name);

        EditorGUI.BeginChangeCheck();

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            NimbatFunctions.GetContactPosition(activeContact),
            activeContact.radius * selectedVRCObject.vrcObject.absoluteScale);

        if (EditorGUI.EndChangeCheck())
        {
            activeContact.radius = newRadius / selectedVRCObject.vrcObject.absoluteScale;

            if (mirrorRadius)
            {
                mirrorContact.radius = newRadius / mirroredVRCObject.vrcObject.absoluteScale;
            }
        }
            if (mirrorPosition)
            {
                Vector3 mirroredPosition = activeContact.transform.localPosition;
                mirroredPosition.x *= -1;

                mirrorContact.transform.localPosition = mirroredPosition;
            }

        //draws the mirrored contact
        if (mirrorObject)
        {
            
            if (mirroredVRCObject.vrcObject.mirrorType == MirrorTypes.Right)
            {
                Handles.color = rightSideColor;
            }
            if (mirroredVRCObject.vrcObject.mirrorType == MirrorTypes.Left)
            {
                Handles.color = leftSideColor;
            }
            
            Handles.SphereHandleCap(1,
                NimbatFunctions.GetContactPosition(mirrorContact),
                Quaternion.identity,
                (mirrorContact.radius * 2) * mirroredVRCObject.vrcObject.absoluteScale
                , EventType.Repaint);

            Handles.color = handlesDefaultColor;
        }

    }

    static public void DrawSelectedPhysboneHandles()
    {

        //--for some reason, if i dont draw this handle first, nothing else works
        //Dan said this line is very important, this is not a meme or a joke
        Handles.Label(Vector3.zero, "");

        
        //for( int i = 0; i< NimbatPhysBoneDrawer.physBoneTransform.Count; i++)
        foreach(Transform physTransform in NimbatPhysBoneDrawer.physBoneTransform)
        {
            NimbatPhysBoneDrawer.DrawTestPhysboneOrientAxes(physTransform);
        }            
        
        /*
        foreach (NimbatPhysBoneSegment physSegment in NimbatPhysBoneDrawer.physBoneSegments)
        {
            DrawTestPhysboneSegment(physSegment);
        }
        */
        
        AnimationCurve tempCurve = activePhysbone.radiusCurve;

        for (int i = 0; i < tempCurve.keys.Length; i++)
        {
            if(i == selectedCurveKey)
            {
                continue;
            }

            float keyValue = tempCurve.keys[i].value;
            Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(tempCurve.keys[i].time);
            float radiusScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(tempCurve.keys[i].time) * keyValue) * activePhysbone.radius;


            if (NimbatPhysBoneDrawer.DrawCurveKeyPoint(keyPosition, radiusScale))
            {
                selectedCurveKey = i;
            }
        }



        DrawCurvePointHandles();


        //Handles.SphereHandleCap(1, NimbatPhysBoneDrawer.GetPosition(physBoneTestSlider), Quaternion.identity, .1f, EventType.Repaint);
        
    }

    static void DrawCurvePointHandles()
    {
        int id = selectedCurveKey;

        AnimationCurve tempCurve = activePhysbone.radiusCurve;

        keyframeData = tempCurve.keys[id];

        float keyValue = tempCurve.keys[id].value;
        Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(tempCurve.keys[id].time);
        float absoluteScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(tempCurve.keys[id].time));
        float scaledValue = keyValue * activePhysbone.radius;

        Handles.color = Color.green;

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            keyPosition,
            scaledValue * absoluteScale);

        //float outTangentAngle = Handles. tempCurve.keys[id].inTangent;

        Vector3 outHandlePosition = NimbatPhysBoneDrawer.GetForwardDirectionAtPosition(tempCurve.keys[id].time);
        Vector3 outHandleRotationVector = NimbatPhysBoneDrawer.GetRotationAxisAtPosition(tempCurve.keys[id].time);

        outTanValue = tempCurve.keys[id].outTangent / NimbatPhysBoneDrawer.physBone_Lenght;

        outHandleAngle =  Mathf.Rad2Deg * Mathf.Atan(outTanValue * (NimbatPhysBoneDrawer.physBone_Lenght * .5f));


        outHandlePosition = NimbatFunctions.RotateVectorOnAxis(outHandlePosition, outHandleRotationVector, outHandleAngle);

        HandlesUtil.DrawWireSphere(keyPosition + (outHandlePosition * .1f), .005f);
        HandlesUtil.DrawWireSphere(keyPosition - (outHandlePosition * .1f), .005f);
        HandlesUtil.DrawWireSphere(keyPosition, .005f);

        Handles.DrawLine(keyPosition, keyPosition + (outHandlePosition * .1f));
        Handles.DrawLine(keyPosition, keyPosition - (outHandlePosition * .1f));

        Handles.Disc(0, Quaternion.identity, keyPosition, outHandleRotationVector, .5f, false, 0);

              

        Keyframe newKey = new Keyframe();
        newKey.value = ((newRadius / activePhysbone.radius) / absoluteScale);
        newKey.time = tempCurve.keys[id].time;
        newKey.inTangent = tempCurve.keys[id].inTangent;
        newKey.inWeight = tempCurve.keys[id].inWeight;
        newKey.outTangent = tempCurve.keys[id].outTangent;
        newKey.outWeight = tempCurve.keys[id].outWeight;



        activePhysbone.radiusCurve.MoveKey(id, newKey);
    }
}
