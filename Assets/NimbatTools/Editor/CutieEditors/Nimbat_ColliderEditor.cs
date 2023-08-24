using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class Nimbat_ColliderEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedVRCObject;
    VRCPhysBoneColliderBase activeCollider;


    bool toggleHeightHandle = true;
    bool toggleRadiusHandle = true;

    bool toggleOffsetEditing;
    bool toggleOffsetEditingFirstTime;

    Vector3 capsuleDirection;
    Vector3 handlesPosition;
    Vector3 newPosition;

    float newRadius;

    //--data for offset handles
    Vector3 localOffset;
    Quaternion finalRotation;

    #region ============================ constructor / destructor
    public Nimbat_ColliderEditor()
    {
        title = "VRC Physbone Collider";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 60;

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
        EditorApplication.hierarchyChanged += OnSelectionChanged;
    }
    ~Nimbat_ColliderEditor()
    {
        Nimbat_SelectionData.OnSelectionChanged -= OnSelectionChanged;
        EditorApplication.hierarchyChanged -= OnSelectionChanged;
    }

    #endregion

    public override bool IsWindowValid()
    {
        if(selectedVRCObject.vrcObjectType == VRCObjectType.Collider)
        {
            return true;
        }
        return false;
    }

    void OnSelectionChanged()
    {
        activeCollider = null;
        selectedVRCObject.ClearData();

        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;

        if (selectedVRCObject.vrcObjectType == VRCObjectType.Collider)
        {
            activeCollider = selectedVRCObject.collider;
        }
    }


    public override void CutieInspectorContent()
    {
        if (selectedVRCObject.collider == null)
        {
            return;
        }

        GUILayout.BeginHorizontal();
        toggleHeightHandle = GUILayout.Toggle(toggleHeightHandle, "height controller");
        toggleRadiusHandle = GUILayout.Toggle(toggleRadiusHandle, "radius controller");

        GUILayout.EndHorizontal();
        toggleOffsetEditing = GUILayout.Toggle(toggleOffsetEditing, "Edit vrc offsets instead of transforms");

        if (toggleOffsetEditing != toggleOffsetEditingFirstTime)
        {
            if (toggleOffsetEditing)
            {
                Tools.current = Tool.Move;
            }
            toggleOffsetEditingFirstTime = toggleOffsetEditing;
        }

        if (GUILayout.Button("Reset vrc offset data"))
        {
            if (EditorUtility.DisplayDialog("revert offset data", "are you sure you want to revert vrc object position and rotation offset data back to 0?", "uwu yes"))
            {
                selectedVRCObject.positionOffset = Vector3.zero;
                selectedVRCObject.rotationOffset = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    public override void CutieInspectorHandles()                
    {
        if (!activeCollider)
        {
            return;
        }

        if(activeCollider.shapeType == VRCPhysBoneColliderBase.ShapeType.Plane)
        {
            return;
        }

        Handles.Label(Vector3.zero, string.Empty);

        EditorGUI.BeginChangeCheck();
        if (toggleRadiusHandle)
        {
            newRadius = Handles.RadiusHandle(
                Quaternion.identity,
                selectedVRCObject.positionFinal,
                activeCollider.radius * selectedVRCObject.absoluteScale);

            if (EditorGUI.EndChangeCheck())
            {
                activeCollider.radius = newRadius / selectedVRCObject.absoluteScale;
            }
        }

        if (toggleOffsetEditing)
        {
            Tools.hidden = true;

            if (Tools.current == Tool.Move)
            {
                localOffset = selectedVRCObject.position + selectedVRCObject.positionOffset;
                if (Tools.pivotRotation == PivotRotation.Local)
                {
                    localOffset = Handles.PositionHandle(localOffset, selectedVRCObject.rotationFinal);
                }
                else
                {
                    localOffset = Handles.PositionHandle(localOffset, Quaternion.identity);
                }

                selectedVRCObject.positionOffset = (localOffset - selectedVRCObject.position);
            }
            if (Tools.current == Tool.Rotate)
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

        if (toggleHeightHandle)
        {            
            capsuleDirection = Vector3.Normalize( selectedVRCObject.rotationFinal * Vector3.up);
            handlesPosition = selectedVRCObject.positionFinal + ((capsuleDirection.normalized * (activeCollider.height / 2)) * selectedVRCObject.absoluteScale);
            newPosition = Handles.Slider(handlesPosition, capsuleDirection);

            float distance = Vector3.Distance(selectedVRCObject.positionFinal, newPosition);

            activeCollider.height = (distance * 2) / selectedVRCObject.absoluteScale;
        }

    }

}
