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
                selectedVRCObject.contact.position = Vector3.zero;
                selectedVRCObject.contact.rotation = Quaternion.Euler(Vector3.zero);
            }
        }
    }

    public override void CutieInspectorHandles()                
    {
        if (!activeCollider)
        {
            return;
        }



        Handles.Label(Vector3.zero, "Collider");

        switch (activeCollider.shapeType)
        {
            case VRCPhysBoneColliderBase.ShapeType.Sphere:
            case VRCPhysBoneColliderBase.ShapeType.Capsule:

                selectedVRCObject.vrcRadius_Scaled = Handles.RadiusHandle(Quaternion.identity, selectedVRCObject.positionFinal, selectedVRCObject.vrcRadius_Scaled);

                capsuleDirection = activeCollider.transform.TransformDirection(Vector3.up).normalized;
                handlesPosition = selectedVRCObject.positionFinal + ((capsuleDirection * (activeCollider.height * .5f)) * selectedVRCObject.absoluteScale);        
                newPosition = Handles.Slider(handlesPosition, capsuleDirection, .03f, Handles.ConeHandleCap, 0);

                float distance = Vector3.Distance(selectedVRCObject.positionFinal, newPosition);

                activeCollider.height = (distance * 2) / selectedVRCObject.absoluteScale;

                if (activeCollider.height > activeCollider.radius * 2f)
                {
                    activeCollider.shapeType = VRCPhysBoneColliderBase.ShapeType.Capsule;
                }
                else
                {
                    activeCollider.shapeType = VRCPhysBoneColliderBase.ShapeType.Sphere;
                }

                break;                
        }        
    }

}
