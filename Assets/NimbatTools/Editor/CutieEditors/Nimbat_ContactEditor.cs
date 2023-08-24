using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;


/// <summary>
/// Class in charge of showing editor handles for editing contact properties faster
/// </summary>
public class Nimbat_ContactEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedVRCObject;
    
    bool toggleHeightHandle = true;
    bool toggleRadiusHandle = true;

    bool toggleOffsetEditing;
    bool toggleOffsetEditingFirstTime;

    static ContactBase activeContact;
    float newRadius;

    //--Data for Handles
    Vector3 capsuleDirection;
    Vector3 handlesPosition;
    Vector3 newPosition;

    //--data for offset handles
    Vector3 localOffset;
    Quaternion finalRotation;

    #region ============================ constructor / destructor
    public Nimbat_ContactEditor()
    {
        title = "VRC Contact";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 60;

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
    }
    ~Nimbat_ContactEditor()
    {
        Nimbat_SelectionData.OnSelectionChanged -= OnSelectionChanged;
    }
    #endregion


    #region ========================== CutieInspector overrides
    public override void CutieInspectorContent()
    {        
        if(selectedVRCObject.contact == null)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        toggleHeightHandle = GUILayout.Toggle(toggleHeightHandle, "height controller");        
        toggleRadiusHandle = GUILayout.Toggle(toggleRadiusHandle, "radius controller");

        GUILayout.EndHorizontal();
        toggleOffsetEditing = GUILayout.Toggle(toggleOffsetEditing, "Edit vrc offsets instead of transforms");

        if(toggleOffsetEditing != toggleOffsetEditingFirstTime)
        {
            if (toggleOffsetEditing)
            {
                Tools.current = Tool.Move;                
            }
            toggleOffsetEditingFirstTime = toggleOffsetEditing;
        }

        if(GUILayout.Button("Reset vrc offset data"))
        {
            if(EditorUtility.DisplayDialog("revert offset data","are you sure you want to revert vrc object position and rotation offset data back to 0?","uwu yes"))
            {
                selectedVRCObject.contact.position = Vector3.zero;
                selectedVRCObject.contact.rotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    public override void CutieInspectorHandles()
    {
        DrawSelectedContactHandles();
    }

    public override bool IsWindowValid()
    {
        if(Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.Contact)
        {
            return true;
        }

        return false;
    }

    #endregion

    /// <summary>
    /// If the object we have selected is a contact, we draw the handles to edit it
    /// </summary>
    public void DrawSelectedContactHandles()
    {
        Handles.Label(Vector3.zero, string.Empty);  

        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType != VRCObjectType.Contact || !activeContact)
        {
            return;
        }

        //--draws name of contact
        Handles.Label(NimbatFunctions.GetContactPosition(activeContact), activeContact.transform.name);
        Handles.DrawDottedLine(selectedVRCObject.position, selectedVRCObject.positionFinal, 4f);

        EditorGUI.BeginChangeCheck();
        if (toggleRadiusHandle)
        {
            newRadius = Handles.RadiusHandle(
                Quaternion.identity,
                selectedVRCObject.positionFinal,
                activeContact.radius * selectedVRCObject.absoluteScale);

            if (EditorGUI.EndChangeCheck())
            {                
                activeContact.radius = newRadius / selectedVRCObject.absoluteScale;
            }
        }


        if (toggleOffsetEditing)
        {            
            Tools.hidden = true;
            
            if(Tools.current == Tool.Move)
            {
                localOffset = selectedVRCObject.position + selectedVRCObject.positionOffset;
                if(Tools.pivotRotation == PivotRotation.Local)
                {
                    localOffset = Handles.PositionHandle(localOffset, selectedVRCObject.rotationFinal);
                }
                else
                {
                    localOffset = Handles.PositionHandle(localOffset, Quaternion.identity);
                }

                selectedVRCObject.positionOffset = (localOffset - selectedVRCObject.position);
            }
            if(Tools.current == Tool.Rotate)
            {
                finalRotation = selectedVRCObject.rotationOffset;                
                finalRotation = Handles.RotationHandle(finalRotation, selectedVRCObject.positionFinal);

                selectedVRCObject.rotationOffset = finalRotation.normalized;
            }
        }
        else
        {
            Tools.hidden = false;
        }

        if(toggleHeightHandle)
        {
            capsuleDirection = Vector3.Normalize(selectedVRCObject.rotationFinal * Vector3.up);
            handlesPosition = selectedVRCObject.positionFinal + ((capsuleDirection.normalized * (activeContact.height / 2)) * selectedVRCObject.absoluteScale);
            newPosition = Handles.Slider(handlesPosition, capsuleDirection);

            float distance = Vector3.Distance(selectedVRCObject.positionFinal, newPosition);

            activeContact.height = (distance*2) / selectedVRCObject.absoluteScale;            
        }

    }

    /// <summary>
    /// Function called by Nimbat_SelectionData, we need to always get the data from
    /// a source of truth which is Nimbat_SelectionData and it gets the data from
    /// Nimbat_MirrorObjectInspector
    /// </summary>
    public void OnSelectionChanged()
    {
        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;
        activeContact = Nimbat_SelectionData.selectedVRCNimbatObject.contact;

        Tools.hidden = false;
        toggleOffsetEditing = false;
    }

  
}