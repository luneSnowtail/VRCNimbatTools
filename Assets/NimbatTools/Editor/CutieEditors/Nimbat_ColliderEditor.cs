using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class Nimbat_ColliderEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedObject;
    VRCPhysBoneColliderBase activeCollider;

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
        if(selectedObject.vrcObjectType == VRCObjectType.Collider)
        {
            return true;
        }
        return false;
    }

    void OnSelectionChanged()
    {
        activeCollider = null;
        selectedObject.ClearData();

        selectedObject = Nimbat_SelectionData.selectedVRCNimbatObject;

        if (selectedObject.vrcObjectType == VRCObjectType.Collider)
        {
            activeCollider = selectedObject.collider;
        }
    }


    public override void CutieInspectorContent()
    {
        
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

                selectedObject.vrcRadius_Scaled = Handles.RadiusHandle(Quaternion.identity, selectedObject.positionFinal, selectedObject.vrcRadius_Scaled);

                capsuleDirection = activeCollider.transform.TransformDirection(Vector3.up).normalized;
                handlesPosition = selectedObject.positionFinal + ((capsuleDirection * (activeCollider.height * .5f)) * selectedObject.absoluteScale);        
                newPosition = Handles.Slider(handlesPosition, capsuleDirection, .03f, Handles.ConeHandleCap, 0);

                float distance = Vector3.Distance(selectedObject.positionFinal, newPosition);

                activeCollider.height = (distance * 2) / selectedObject.absoluteScale;

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
