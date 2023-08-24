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
public class Nimbat_MirrorEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedVRCObject;
   
    NimbatMirrorObject mirrorGroup;

    GameObject activeObject;
    GameObject mirrorObject;
    Transform possibleMirror;

    bool mirrorTransforms;
    bool showMirrorObject = true;
    bool mirrorData;

    static public List<ContactBase> avatarContacts;
    static public List<NimbatMirrorObject> mirrorContactsList;


    #region ============================ constructor / destructor
    public Nimbat_MirrorEditor()
    {
        title = "VRC Mirrored object";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 110;

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
    }
    ~Nimbat_MirrorEditor()
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
        selectedVRCObject.ClearData();
        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;
        activeObject = selectedVRCObject.gameObject;
        mirrorGroup = Nimbat_SelectionData.nimbatMirrorData;
        possibleMirror = null;

        if(selectedVRCObject.vrcObjectType == VRCObjectType.None)
        {
            return;
        }

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
        
        if (!mirrorObject)
        {
            if (activeObject)
            {
                possibleMirror = NimbatFunctions.GetTransformInChild(Nimbat_AvatarSettings.selectedAvatar.gameObject, NimbatFunctions.MirrorNameSuffix(activeObject.name));
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
            if (possibleMirror)
            {
                GUILayout.Label("possible mirror?" + possibleMirror.name);
                if (GUILayout.Button("Add Missing components"))
                {
                    CreateMirrorGameobject();
                }
            }
            else
            {
                GUILayout.Label("no mirror gameobject found");
                if(GUILayout.Button("Create mirror Gameobject"))
                {
                    CreateMirrorGameobject();
                }
            }

            return;
        }

        showMirrorObject = GUILayout.Toggle(showMirrorObject, "Show mirror object");

        mirrorTransforms = GUILayout.Toggle(mirrorTransforms, "Realtime mirror position");
        mirrorData = GUILayout.Toggle(mirrorData, "Realtime Mirror data");

        if (GUILayout.Button("Copy to mirror object"))
        {
            CreateOrCopyDataToMirror();
        }
    }

    public override void CutieInspectorHandles()
    {
        if (!activeObject)
            return;

        if (!mirrorObject)
            return;

        if(mirrorTransforms)
        {
            TransferTransformMirrorData();
        }

        if (mirrorData)
        {
            CreateOrTransferMirrorData();
        }

        if (showMirrorObject)
        {
            switch (selectedVRCObject.mirrorType)
            {
                case MirrorTypes.Left:
                    Handles.color = Color.blue;
                    NimbatHandles.DrawVRCNimbatObject(mirrorGroup.vrcObject_Right);
                    break;
                case MirrorTypes.Right:
                    Handles.color = Color.red;
                    NimbatHandles.DrawVRCNimbatObject(mirrorGroup.vrcObject_Left);
                    break;
            }
            Handles.color = NimbatCore.handlesDefaultColor;
        }
    }

    void CreateMirrorGameobject()
    {
        if (possibleMirror)
        {
            mirrorObject = possibleMirror.gameObject;
        }
        else
        {
            if (!activeObject.transform.parent)
            {
                Debug.Log("gameobject does not belongs to any avatar");
                return;
            }

            string parentName = activeObject.transform.parent.name;
            parentName = NimbatFunctions.MirrorNameSuffix(parentName);

            Transform parentGO = GameObject.Find(parentName).transform;

            mirrorObject = new GameObject();
            mirrorObject.name = NimbatFunctions.MirrorNameSuffix(activeObject.name);
            mirrorObject.transform.SetParent(parentGO, true);        
        }
        CreateOrCopyDataToMirror();
    }

    void TransferTransformMirrorData()
    {
        Vector3 localPos;
        Vector3 localRot;

        NimbatFunctions.MirrorTransforms(activeObject.transform, out localPos, out localRot);
        mirrorObject.transform.localPosition = localPos;
        mirrorObject.transform.localRotation = Quaternion.Euler(localRot);
        mirrorObject.transform.localScale = activeObject.transform.localScale;
    }

    void CreateOrCopyDataToMirror()
    {
        TransferTransformMirrorData();

        CreateOrTransferMirrorData();
    }

    void CreateOrTransferMirrorData()
    {
        switch (selectedVRCObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                CreateOrTransferMirrorContactData();
                break;
            case VRCObjectType.Collider:
                CreateOrTransformMirrorColliderData();
                break;
            case VRCObjectType.PhysBone:
                CreateOrTransformMirrorPhysboneData();
                break;
        }
    }

    void CreateOrTransformMirrorPhysboneData()
    {
        VRCPhysBoneBase physbone;

        physbone = mirrorObject.GetComponent<VRCPhysBoneBase>();

        //--we only create component if we did not found it
        if (!physbone)
        {
            physbone = mirrorObject.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone>();
        }

        physbone.allowCollision = selectedVRCObject.physBone.allowCollision;
        physbone.allowGrabbing = selectedVRCObject.physBone.allowGrabbing;
        physbone.allowPosing = selectedVRCObject.physBone.allowPosing;

        physbone.radius = selectedVRCObject.physBone.radius;
        physbone.radiusCurve = selectedVRCObject.physBone.radiusCurve;
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
        mirrorContactComponent.height = selectedVRCObject.contact.height;

        mirrorContactComponent.position = NimbatFunctions.MirrorLocalPosition( selectedVRCObject.contact.position);
        mirrorContactComponent.rotation = NimbatFunctions.MirrorRotation( selectedVRCObject.contact.rotation);
        TransferMirrorTags();

    }


    void CreateOrTransformMirrorColliderData()
    {
        VRCPhysBoneColliderBase vrcCollider;

        vrcCollider = mirrorObject.GetComponent<VRCPhysBoneColliderBase>();

        //--we only create component if we did not found it
        if (!vrcCollider)
        {
            vrcCollider = mirrorObject.AddComponent<VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBoneCollider>();
        }

        vrcCollider.radius = selectedVRCObject.collider.radius;
        vrcCollider.height = selectedVRCObject.collider.height;
        vrcCollider.shape = selectedVRCObject.collider.shape;
        vrcCollider.shapeType = selectedVRCObject.collider.shapeType;
        vrcCollider.insideBounds = selectedVRCObject.collider.insideBounds;

        vrcCollider.position = NimbatFunctions.MirrorLocalPosition(selectedVRCObject.collider.position);
        vrcCollider.rotation = NimbatFunctions.MirrorRotation( selectedVRCObject.collider.rotation);
    }

    /// <summary>
    /// it copies the tags from the selected contact and sends them to the mirrored gameobject
    /// </summary>
    public void TransferMirrorTags()
    {
        ContactBase mirrorContact;

        mirrorContact = mirrorObject.GetComponent<ContactBase>();
        if (!mirrorContact)
        {
            Debug.LogWarning("Nimbat tools: we dont have a mirror contact in this group");
            return;
        }

        mirrorContact.collisionTags = new List<string>();

        if (selectedVRCObject.contact.collisionTags.Count <= 0)
        {
            Debug.LogWarning("Nimbat tools: selected contact has no tags to mirror");
            return;
        }

        bool hasMirrorSuffix;
        bool isRight;
        string currentTag;
        mirrorContact.collisionTags = new List<string>();

        for (int i = 0; i < selectedVRCObject.contact.collisionTags.Count; i++)
        {
            currentTag = selectedVRCObject.contact.collisionTags[i];

            hasMirrorSuffix = NimbatFunctions.NameHasMirrorSuffix(currentTag, out isRight);

            if (hasMirrorSuffix)
            {
                mirrorContact.collisionTags.Add(NimbatFunctions.GetTagMirrorSuffix(currentTag));
            }
            else
            {
                mirrorContact.collisionTags.Add(currentTag);
            }
        }       
    }
}
