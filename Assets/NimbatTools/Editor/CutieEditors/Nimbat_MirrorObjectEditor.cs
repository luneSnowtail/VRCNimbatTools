using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

/// <summary>
/// This class is the one that takes charge of linking the objects with their respective mirror data
/// and drawing mirror handles, in previous versions each editor would deal with their mirrors but
/// its easier if this class just takes care of mirroring data;
/// </summary>
public class Nimbat_MirrorObjectEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedVRCObject;
    GameObject activeObject;

    GameObject mirrorObject;    
    bool mirrorPosition;

    static public List<ContactBase> avatarContacts;
    static public List<NimbatMirrorObject> mirrorContactsList;


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
        
        if(Nimbat_SelectionData.selectedVRCNimbatObject.mirrorType != MirrorTypes.None && Nimbat_SelectionData.selectedVRCNimbatObject.gameObject != null)
        {
            return true;
        }
        return false;
    }


    void OnSelectionChanged()
    {
        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;
        activeObject = selectedVRCObject.gameObject;

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
            if(GUILayout.Button("Create mirror Gameobject"))
            {
                CreateMirrorGameobject();
            }

            return;
        }

        mirrorPosition = GUILayout.Toggle(mirrorPosition, " Mirror position");

        if (GUILayout.Button("Copy VRC Object Data"))
        {
            CreateOrCopyDataToMirror();
        }
    }

    public override void CutieInspectorHandles()
    {
        if (!activeObject)
            return;

        if(mirrorPosition && mirrorObject)
        {
            mirrorObject.transform.localPosition = NimbatFunctions.MirrorLocalPosition(activeObject.transform.localPosition);
        }
    }

    void CreateMirrorGameobject()
    {
        if (!activeObject.transform.parent)
        {
            Debug.Log("gameobject does not belongs to any avatar");
            return;
        }

        string parentName = activeObject.transform.parent.name;
        Transform parentGO = GameObject.Find(parentName).transform;

        mirrorObject = new GameObject();
        mirrorObject.name = NimbatFunctions.MirrorNameSuffix(activeObject.name);
        mirrorObject.transform.SetParent(parentGO, true);

        Vector3 localPos;
        Vector3 localRot;

        NimbatFunctions.MirrorTransforms(activeObject.transform, out localPos, out localRot);
        mirrorObject.transform.localPosition = localPos;
        mirrorObject.transform.localRotation = Quaternion.Euler(localRot);
        mirrorObject.transform.localScale = activeObject.transform.localScale;

        CreateOrCopyDataToMirror();
    }

    void CreateOrCopyDataToMirror()
    {
        switch (selectedVRCObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                CreateOrTransferMirrorContactData();
                break;
        }
    }

    void CreateOrTransferMirrorContactData()
    {
        ContactBase mirrorContactComponent;

        if (selectedVRCObject.contactType == ContactType.Receiver)
        {
            ContactReceiver mirrorContactReceiverComponent = mirrorObject.GetComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
            if (!mirrorContactReceiverComponent)
            {
                mirrorContactReceiverComponent = mirrorObject.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
            }

            mirrorContactComponent = mirrorContactReceiverComponent;

            mirrorContactReceiverComponent.receiverType = selectedVRCObject.receiver.receiverType;
            mirrorContactReceiverComponent.minVelocity = selectedVRCObject.receiver.minVelocity;

            if (!string.IsNullOrWhiteSpace(mirrorContactReceiverComponent.parameter))
            {
                mirrorContactReceiverComponent.parameter = NimbatFunctions.GetTagMirrorSuffix(selectedVRCObject.receiver.parameter);
            }

        }
        else
        {
            mirrorContactComponent = mirrorObject.GetComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();

            //--we only create component if we did not found it
            if (!mirrorContactComponent)
            {
                mirrorContactComponent = mirrorObject.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();
            }
        }

        mirrorContactComponent.shapeType = selectedVRCObject.contact.shapeType;
        mirrorContactComponent.radius = selectedVRCObject.contact.radius;
        mirrorContactComponent.position = selectedVRCObject.contact.position;
        mirrorContactComponent.rotation = selectedVRCObject.contact.rotation;
        mirrorContactComponent.height = selectedVRCObject.contact.height;

    }
}
