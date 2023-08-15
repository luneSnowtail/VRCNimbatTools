using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

/*
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
*/

/// <summary>
/// Class in charge of showing editor handles for editing contact properties faster
/// </summary>
public class Nimbat_ContactEditor : NimbatCutieInspectorWindow
{
    NimbatVRCObjectBase selectedVRCObject;
    static NimbatVRCObjectBase mirroredVRCObject;

    static bool toggleHeightHandle = true;
    static bool toggleRadiusHandle = true;

    static bool toggleOffsetEditing;
    bool toggleOffsetEditingFirstTime;

    static ContactBase activeContact;
    float newRadius;

    /*
    static ContactReceiver activeReceiver;
    static ContactType activeContactType;
    static ContactBase mirrorContact;
    static List<Tag> tags;  
    static VRCDefaultTags defaultTempTag;
    */

    //--Data for Handles
    Vector3 capsuleDirection;
    Vector3 handlesPosition;
    Vector3 newPosition;

    //--data for offset handles
    Vector3 localOffset;
    Quaternion transformRotation;
    Quaternion finalRotation;
    Quaternion offsetRotation;

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

        #region ============ OLD MIRROR DATA
        /*
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
        */
        #endregion

        //--this editor (and every other) were going to handle the transfer to mirror
        //  individually but i decided its best to have only one editor in charge of mirror stugg

        #region ================ OLD TAG DATA, prob delete

        /*
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
        */
        #endregion
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
        Handles.Label(Vector3.zero, "");  

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
            capsuleDirection = selectedVRCObject.rotationFinal * Vector3.up;
            handlesPosition = selectedVRCObject.positionFinal + ((capsuleDirection * (activeContact.height / 2)) * selectedVRCObject.absoluteScale);
            newPosition = Handles.Slider(handlesPosition, capsuleDirection, .03f ,Handles.ConeHandleCap,0);

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
        //mirroredVRCObject.contact = null;
        //mirrorContact = null;

        //hasMirrorSuffix = false;
        //hasMirrorData = false;

        selectedVRCObject = Nimbat_SelectionData.selectedVRCNimbatObject;
        activeContact = Nimbat_SelectionData.selectedVRCNimbatObject.contact;

        Tools.hidden = false;
        toggleOffsetEditing = false;

        /*
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
        */
        #region ================================ old mirror code
        /*
         * 
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
        */
        #endregion
    }

    /*
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

    */
}