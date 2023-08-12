using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

/// <summary>
/// Default Vrchat tag values
/// </summary>
public enum VRCDefaultTags
{
    Custom,
    Head,
    Torso,
    Hand,
    HandL,
    HandR,
    Foot,
    FootL,
    FootR,
    Finger,
    FingerL,
    FingerR,
    FingerIndex,
    FingerMiddle,
    FingerRing,
    FingerLittle,
    FingerIndexL,
    FingerMiddleL,
    FingerRingL,
    FingerLittleL,
    FingerIndexR,
    FingerMiddleR,
    FingerRingR,
    FingerLittleR,
};

/// <summary>
/// Struct used to draw the tag labels in the cutie inspector
/// </summary>
[System.Serializable]
public struct Tag
{
    public VRCDefaultTags defaultTags;
    public string tagName;
    public bool hasMirrorSuffix;
}

public class Nimbat_ContactEditor : NimbatCutieInspectorWindow
{
    static Color leftSideColor = new Color(0, 0, 1, .3f);
    static Color rightSideColor = new Color(1, 0, 0, .3f);

    static NimbatVRCObjectBase selectedVRCObject;
    static NimbatVRCObjectBase mirroredVRCObject;

    static bool hasMirrorData;
    static bool hasMirrorSuffix;

    static bool mirrorRadius;
    static bool mirrorPosition;

    static ContactBase activeContact;
    static ContactBase mirrorContact;

    static ContactType activeContactType;
    static ContactReceiver activeReceiver;

    static List<Tag> tags;  

    static VRCDefaultTags defaultTempTag;

    #region ============================ constructor / destructor
    public Nimbat_ContactEditor()
    {
        title = "Mirror Group Contact";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 220;

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

        if (hasMirrorSuffix)
        {
            if (!hasMirrorData)
            {
                if(GUILayout.Button("Create Mirror Contact"))
                {
                    CreateOrTransferMirrorData();
                }
                
            }
            else
            {
                mirrorRadius = GUILayout.Toggle(mirrorRadius, "Mirror radius");
                mirrorPosition = GUILayout.Toggle(mirrorPosition, "Mirror position");
            }
        }        

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("=== Tags ===", EditorStyles.miniLabel);
        if(GUILayout.Button("Add"))
        {            
            activeContact.collisionTags.Add("");
            tags.Add(new Tag());
        }

        GUILayout.EndHorizontal();

        UpdateTags();

        #region ========================= Draw tags

        for (int i = 0; i< tags.Count; i++)
        {
            GUILayout.BeginHorizontal();

            Tag tempTag = new Tag();
            tempTag = tags[i];


            EditorGUILayout.EnumPopup("", defaultTempTag, GUILayout.Width(50), GUILayout.Height(20));
            tempTag.tagName = GUILayout.TextField(tags[i].tagName, GUILayout.Width(120), GUILayout.Height(20));
            tempTag.hasMirrorSuffix = GUILayout.Toggle(tempTag.hasMirrorSuffix, "Mir", EditorStyles.toolbarButton);
            tags[i] = tempTag;

            if(GUILayout.Button("x", EditorStyles.toolbarButton))
            {
                tags.RemoveAt(i);
                activeContact.collisionTags.RemoveAt(i);
            }


            GUILayout.EndHorizontal();     
        }

        #endregion

        if(selectedVRCObject.contactType == ContactType.Receiver)
        {
            GUILayout.Label("", EditorStyles.miniLabel);
            GUILayout.Label("=== Receiver Tag Data ===", EditorStyles.miniLabel);

            GUILayout.BeginHorizontal();

            
            activeReceiver.receiverType = (ContactReceiver.ReceiverType) EditorGUILayout.EnumPopup("", activeReceiver.receiverType, GUILayout.Width(70), GUILayout.Height(20));            
            activeReceiver.parameter = GUILayout.TextField(activeReceiver.parameter, GUILayout.Width(150), GUILayout.Height(20));

            GUILayout.EndHorizontal();
        }
        else
        {

        }

        if (!hasMirrorData)
        {
            GUI.enabled = false;
        }
        else
        {
            GUI.enabled = true;
        }

        

        if(GUILayout.Button("Copy Tag Data to Mirror"))
        {
            CreateOrTransferMirrorData();
        }

        activeContact.collisionTags = new List<string>();
        for(int i = 0; i< tags.Count; i++)
        {
            activeContact.collisionTags.Add(tags[i].tagName);
        }


        GUI.enabled = true;
    }

    public override void CutieInspectorHandles()
    {
        DrawSelectedContactHandle();
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
    static public void DrawSelectedContactHandle()
    {
        Handles.Label(Vector3.zero, "");  

        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType != VRCObjectType.Contact || !activeContact)
        {
            return;
        }

        //--draws name of contact
        Handles.Label(NimbatFunctions.GetContactPosition(activeContact), activeContact.transform.name);

        EditorGUI.BeginChangeCheck();

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            NimbatFunctions.GetContactPosition(activeContact),
            activeContact.radius * selectedVRCObject.absoluteScale);

        if(selectedVRCObject.contact.shapeType == ContactBase.ShapeType.Capsule)
        {
            Vector3 capsuleDirection = activeContact.transform.TransformDirection(Vector3.up).normalized;
            Vector3 handlesPosition = activeContact.transform.position + ((capsuleDirection * (activeContact.height * .5f)) * selectedVRCObject.absoluteScale);
            Vector3 newPosition = Handles.Slider(handlesPosition, capsuleDirection, .05f ,Handles.ConeHandleCap,0);

            float distance = Vector3.Distance(activeContact.transform.position, newPosition);

            activeContact.height = (distance*2) / selectedVRCObject.absoluteScale;
        }

        if (EditorGUI.EndChangeCheck())
        {
            activeContact.radius = newRadius / selectedVRCObject.absoluteScale;

            if (hasMirrorData)
            {
                if (mirrorRadius)
                {
                    mirrorContact.radius = newRadius / selectedVRCObject.absoluteScale;
                }
            }
        }

        if (hasMirrorData)
        {
            /*
            if (mirrorPosition)
            {
                Vector3 mirroredPosition = activeContact.transform.localPosition;
                mirroredPosition.x *= -1;

                mirrorContact.transform.localPosition = mirroredPosition;
            }
            */

            //draws the mirrored contact
            if (mirrorContact)
            {
                if (mirroredVRCObject.mirrorType == MirrorTypes.Right)
                {
                    Handles.color = rightSideColor;
                }
                if (mirroredVRCObject.mirrorType == MirrorTypes.Left)
                {
                    Handles.color = leftSideColor;
                }

                Handles.SphereHandleCap(1,
                    NimbatFunctions.GetContactPosition(mirrorContact),
                    Quaternion.identity,
                    (mirrorContact.radius * 2) * mirroredVRCObject.absoluteScale
                    , EventType.Repaint);

                Handles.color = NimbatCore.handlesDefaultColor;
            }
        }
    }

    /// <summary>
    /// Function called by Nimbat_SelectionData, we need to always get the data from
    /// a source of truth which is Nimbat_SelectionData and it gets the data from
    /// Nimbat_MirrorObjectInspector
    /// </summary>
    public void OnSelectionChanged()
    {
        mirroredVRCObject.contact = null;
        mirrorContact = null;

        hasMirrorSuffix = false;
        hasMirrorData = false;

        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;
        activeContact = Nimbat_SelectionData.selectedVRCNimbatObject.contact;

        activeContactType = selectedVRCObject.contactType;

        if(activeContactType == ContactType.Receiver)
        {   
            if(activeContact != null)
            {
                activeReceiver = activeContact.GetComponent<ContactReceiver>();
            }
            else
            {
                activeReceiver = null;
            }
        }
        else
        {
            activeReceiver = null;
        }

        //--copy tags
        UpdateTags();

        //--we check for mirror information in the active selected object
        if (Nimbat_SelectionData.selectionHasMirrorSuffix)
        {
            hasMirrorSuffix = true;

            switch (selectedVRCObject.mirrorType)
            {
                case MirrorTypes.Left:
                    if (Nimbat_SelectionData.nimbatMirrorData != null)
                    {
                        if (Nimbat_SelectionData.nimbatMirrorData.vrcObject_Right.contact != null)
                        {
                            mirroredVRCObject = Nimbat_SelectionData.nimbatMirrorData.vrcObject_Right;
                            hasMirrorData = true;
                        }
                    }
                    else
                    {
                        mirrorContact = null;
                        hasMirrorData = false;
                        mirroredVRCObject.contact = null;
                    }
                    break;
                case MirrorTypes.Right:
                    if (Nimbat_SelectionData.nimbatMirrorData != null)
                    {
                        if (Nimbat_SelectionData.nimbatMirrorData.vrcObject_Left.contact != null)
                        {
                            mirroredVRCObject = Nimbat_SelectionData.nimbatMirrorData.vrcObject_Left;
                            hasMirrorData = true;
                        }
                    }
                    else
                    {
                        mirrorContact = null;
                        hasMirrorData = false;
                        mirroredVRCObject.contact = null;
                    }
                    break;
                case MirrorTypes.None:
                    mirrorContact = null;
                    hasMirrorData = false;
                    mirroredVRCObject.contact = null;
                    break;
            }
        }
        else
        {
            mirroredVRCObject.contact = null;
            hasMirrorSuffix = false;
            hasMirrorData = false;
            mirrorContact = null;
        }

        if (hasMirrorData)
        {
            mirrorContact = mirroredVRCObject.contact;
        }


    }

    /// <summary>
    /// Reads the tags from the component and creates strucs we can use to draw the 
    /// editor labels
    /// </summary>
    void UpdateTags()
    {
        if (!activeContact)
            return;

        tags = new List<Tag>();       
        if (activeContact.collisionTags.Count > 0)
            for (int i = 0; i < activeContact.collisionTags.Count; i++)
            {
                bool isRight;
                Tag tempTag = new Tag();

                tempTag.tagName = activeContact.collisionTags[i];
                tempTag.hasMirrorSuffix = NimbatFunctions.NameHasMirrorSuffix(tempTag.tagName, out isRight);

                tags.Add(tempTag);
            }
    }

    /// <summary>
    /// If we have mirror suffix but the opposite game objct is missing this function
    /// generates the mirrored game object
    /// </summary>
    static public void CreateOrTransferMirrorData()
    {
        //--first search for a gameobject that already exists
        GameObject mirrorContactGO = GameObject.Find(NimbatFunctions.MirrorNameSuffix(activeContact.transform.name));

        //--if it cant be found we find parent mirror and create a new gameobject
        if (!mirrorContactGO)
        {
            string mirrorParentName = NimbatFunctions.MirrorNameSuffix(activeContact.transform.parent.name);
            GameObject mirrorParent = GameObject.Find(mirrorParentName);

            if (!mirrorParent)
            {
                return;
            }

            mirrorContactGO = new GameObject();

            //--parent new gameobject to mirror parent
            mirrorContactGO.transform.SetParent(mirrorParent.transform, true);


            mirrorContactGO.transform.localPosition = NimbatFunctions.MirrorLocalPosition(activeContact.transform.localPosition);
            mirrorContactGO.transform.localScale = activeContact.transform.localScale;

            //--get the name and mirror the suffix
            mirrorContactGO.name = NimbatFunctions.MirrorNameSuffix(activeContact.gameObject.name);
        }


        ContactBase mirrorContactComponent;
        

        if(selectedVRCObject.contactType == ContactType.Receiver)
        {
            ContactReceiver mirrorContactReceiverComponent = mirrorContactGO.GetComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
            if (!mirrorContactReceiverComponent)
            {
                mirrorContactReceiverComponent = mirrorContactGO.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactReceiver>();
            }

            mirrorContactComponent = mirrorContactReceiverComponent;

            mirrorContactReceiverComponent.receiverType = selectedVRCObject.receiver.receiverType;
            mirrorContactReceiverComponent.minVelocity = selectedVRCObject.receiver.minVelocity;

            if(!string.IsNullOrWhiteSpace(mirrorContactReceiverComponent.parameter))
            {
                mirrorContactReceiverComponent.parameter = NimbatFunctions.GetTagMirrorSuffix(selectedVRCObject.receiver.parameter);
            }

        }
        else
        {
            mirrorContactComponent = mirrorContactGO.GetComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();

            //--we only create component if we did not found it
            if (!mirrorContactComponent)
            {
                mirrorContactComponent = mirrorContactGO.AddComponent<VRC.SDK3.Dynamics.Contact.Components.VRCContactSender>();
            }
        }

        mirrorContactComponent.radius = activeContact.radius;
        mirrorContactComponent.position = activeContact.position;
        mirrorContactComponent.shapeType = activeContact.shapeType;
        mirrorContactComponent.rotation = activeContact.rotation;

        mirrorContact = mirrorContactComponent;

        //--transfer tags as well
        TransferMirrorTags();

        Selection.activeGameObject = mirrorContactGO;
    }

    /// <summary>
    /// it copies the tags from the selected contact and sends them to the mirrored gameobject
    /// </summary>
    static public void TransferMirrorTags()
    {
        if (!mirrorContact)
        {
            Debug.LogWarning("Nimbat tools: we dont have a mirror contact in this group");
            return;
        }

        mirrorContact.collisionTags = new List<string>();

        if(tags.Count <= 0)
        {
            Debug.LogWarning("Nimbat tools: selected contact has no tags to mirror");
            return;
        }

        for(int i = 0; i < tags.Count; i++)
        {
            if (tags[i].hasMirrorSuffix)
            {
                mirrorContact.collisionTags.Add(NimbatFunctions.GetTagMirrorSuffix(tags[i].tagName));
            }
            else
            {
                mirrorContact.collisionTags.Add(tags[i].tagName);
            }
        }

        mirrorContact.radius = activeContact.radius;
        mirrorContact.height = activeContact.height;
        mirrorContact.shapeType = activeContact.shapeType;

        Vector3 localPos;
        Vector3 localRot;

        NimbatFunctions.MirrorTransforms(activeContact.transform, out localPos, out localRot);

        mirrorContact.transform.localPosition = localPos;
        mirrorContact.transform.localRotation = Quaternion.Euler(localRot);

        Vector3 mirrorRotationEuler = activeContact.rotation.eulerAngles;
        mirrorRotationEuler.y *= -1;
        mirrorRotationEuler.z *= -1;

        mirrorContact.rotation = Quaternion.Euler(mirrorRotationEuler);
            

    }
}