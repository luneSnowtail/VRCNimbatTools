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
    NimbatMirrorObject mirrorGroup;

    GameObject activeObject;

    GameObject mirrorObject;    
    
    bool mirrorTransforms;
    

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
        mirrorGroup = Nimbat_SelectionData.nimbatMirrorData;

        switch (selectedVRCObject.mirrorType)
        {
            case MirrorTypes.Left:
                mirrorObject = mirrorGroup.vrcObject_Right.gameObject;
                break;
            case MirrorTypes.Right:
                mirrorObject = mirrorGroup.vrcObject_Left.gameObject;
                break;
            case MirrorTypes.None:
                mirrorObject = null;
                break;
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

        mirrorTransforms = GUILayout.Toggle(mirrorTransforms, " Mirror position", EditorStyles.toggleGroup);

        if (GUILayout.Button("Copy VRC Object Data"))
        {
            CreateOrCopyDataToMirror();
        }
    }

    public override void CutieInspectorHandles()
    {
        if (!activeObject)
            return;

        if(mirrorTransforms && mirrorObject)
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
            case VRCObjectType.Collider:
                CreateOrTransformMirrorColliderData();
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


    void CreateOrTransformMirrorColliderData()
    {
        VRCPhysBoneColliderBase vrcCollider;

        vrcCollider = mirrorObject.GetComponent<VRCPhysBoneColliderBase>();

        //--we only create component if we did not found it
        if (!vrcCollider)
        {
            vrcCollider = mirrorObject.AddComponent<VRCPhysBoneColliderBase>();
        }

        vrcCollider.radius = selectedVRCObject.collider.radius;
        vrcCollider.height = selectedVRCObject.collider.height;
        vrcCollider.shape = selectedVRCObject.collider.shape;
        vrcCollider.shapeType = selectedVRCObject.collider.shapeType;
        vrcCollider.insideBounds = selectedVRCObject.collider.insideBounds;
        vrcCollider.position = selectedVRCObject.collider.position;
        vrcCollider.rotation = selectedVRCObject.collider.rotation;
    }
}
