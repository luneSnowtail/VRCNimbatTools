using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// This class is the one that takes charge of linking the objects with their respective mirror data
/// and drawing mirror handles, in previous versions each editor would deal with their mirrors but
/// its easier if this class just takes care of mirroring data;
/// </summary>
public class Nimbat_MirrorObjectEditor : NimbatCutieInspectorWindow
{
    GameObject activeObject;
    GameObject mirrorObject;

    bool mirrorPosition;


    #region ============================ constructor / destructor
    public Nimbat_MirrorObjectEditor()
    {
        title = "VRC Mirrored object";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 220;

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
    }
    ~Nimbat_MirrorObjectEditor()
    {
        Nimbat_SelectionData.OnSelectionChanged -= OnSelectionChanged;
    }
    #endregion

    public override bool IsWindowValid()
    {
        if(Nimbat_SelectionData.selectedVRCNimbatObject.mirrorType != MirrorTypes.None && Selection.activeObject != null)
        {
            return true;
        }
        return false;
    }


    void OnSelectionChanged()
    {
        activeObject = Nimbat_SelectionData.selectedVRCNimbatObject.gameObject;

        if (Nimbat_SelectionData.nimbatMirrorData != null)
        {
            if (Nimbat_SelectionData.selectedVRCNimbatObject.mirrorType == MirrorTypes.Left)
            {
                mirrorObject = Nimbat_SelectionData.nimbatMirrorData.vrcObject_Right.gameObject;
            }
            else
            {
                mirrorObject = Nimbat_SelectionData.nimbatMirrorData.vrcObject_Left.gameObject;
            }
        }

    }

    public override void CutieInspectorContent()
    {
        GUILayout.Label("==== Mirror Data ====", EditorStyles.miniLabel);

        if (mirrorObject)
        {
            GUILayout.Label("mirror - " + mirrorObject.name, EditorStyles.miniLabel);
        }
        else
        {
            GUILayout.Label("no mirror gameobject found");
            return;
        }

        mirrorPosition = GUILayout.Toggle(mirrorPosition, " Mirror position");
    }

    public override void CutieInspectorHandles()
    {
        base.CutieInspectorHandles();

        if(mirrorPosition && mirrorObject)
        {
            mirrorObject.transform.localPosition = NimbatFunctions.MirrorLocalPosition(activeObject.transform.localPosition);
        }
    }
}
