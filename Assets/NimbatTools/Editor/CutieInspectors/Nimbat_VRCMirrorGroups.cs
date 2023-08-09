using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;


/// <summary>
/// This class contains the data of 2 mirrored objects and their relationship
/// this class is in charge of mirroring data / parameters, etc.
/// </summary>


[System.Serializable]
public class Nimbat_VRCMirrorGroups : NimbatCutieInspectorWindow
{

    Vector2 scrollPosition;

    static Texture2D buttonIcon_visible;
    static Texture2D buttonIcon_NotVisible;

    bool _tabPhysbones = true;
    bool _tabContacts;
    bool _tabColliders;

    bool tabPhysbones = true;
    bool tabContacts;
    bool tabColliders;


    public Nimbat_VRCMirrorGroups()
    {
        title = "VRC Mirror Groups";
        width = NimbatData.inspectorWindowsWidth;
        height = 250;

        buttonIcon_visible = (Texture2D) Resources.Load("object_visible");
        buttonIcon_NotVisible = (Texture2D)Resources.Load("object_noVisible");
        mainButtonIconPath = "Button_vrcObjects";

        NimbatCore.OnHierarchyChanged += RefreshVRCObjectData;
    }

    ~Nimbat_VRCMirrorGroups()
    {
        NimbatCore.OnHierarchyChanged -= RefreshVRCObjectData;
    }

    #region ========================== CutieInspectorOVerrides
    public override void CutieInspectorHandles()
    {
        DrawMirroredObjects();
    }

    public override void CutieInspectorContent()
    {        
        GUILayout.BeginHorizontal();

        tabPhysbones = GUILayout.Toggle(tabPhysbones, "Physbones",EditorStyles.toolbarButton);
        tabContacts = GUILayout.Toggle(tabContacts, "Contacts", EditorStyles.toolbarButton);
        tabColliders = GUILayout.Toggle(tabColliders, "Colliders", EditorStyles.toolbarButton);

        if (_tabPhysbones != tabPhysbones)
        {
            _tabPhysbones = true;
            tabPhysbones = true;

            if (!NimbatCore.shiftDown)
            {
                tabContacts = false;
                _tabContacts = false;
                tabColliders = false;
                _tabColliders = false;
            }

        }

        if (_tabContacts != tabContacts)
        {
            tabContacts = true;
            _tabContacts = true;
            if (!NimbatCore.shiftDown)
            {
                tabPhysbones = false;
                _tabPhysbones = false;
                tabColliders = false;
                _tabColliders = false;
            }

        }

        if (_tabColliders != tabColliders)
        {
            tabColliders = true;
            _tabColliders = true;
            if (!NimbatCore.shiftDown)
            {
                tabContacts = false;
                _tabContacts = false;
                tabPhysbones = false;
                _tabPhysbones = false;
            }
        }

        GUILayout.EndHorizontal();



        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if(NimbatMirrorData.mirrorContactsList == null || NimbatMirrorData.mirrorContactsList.Count <= 0)
        {
            GUILayout.EndScrollView();
            return;
        }

        for(int i = 0; i< NimbatMirrorData.mirrorContactsList.Count; i++)
        {
            GUILayout.BeginHorizontal();


            if (NimbatMirrorData.mirrorContactsList[i].showInScene)
            {
                NimbatMirrorData.mirrorContactsList[i].showInScene = GUILayout.Toggle(NimbatMirrorData.mirrorContactsList[i].showInScene, buttonIcon_visible, EditorStyles.toolbarButton, GUILayout.Width(20), GUILayout.Height(20));
            }
            else
            {
                NimbatMirrorData.mirrorContactsList[i].showInScene = GUILayout.Toggle(NimbatMirrorData.mirrorContactsList[i].showInScene, buttonIcon_NotVisible, EditorStyles.toolbarButton, GUILayout.Width(20), GUILayout.Height(20));
            }

            

            GUILayout.Label(NimbatMirrorData.mirrorContactsList[i].groupName, GUILayout.Width(150), GUILayout.Height(20));

            if (NimbatMirrorData.mirrorContactsList[i].mirrorGroupValid)
            {
                //--before drawing the button to select an object we make sure it exist
                if (!NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("_L"))
                {                
                    Selection.activeGameObject = NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact.gameObject;
                }
                GUI.enabled = true;

                //--before drawing the button to select an object we make sure it exist
                if (!NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button("_R"))
                {                
                    Selection.activeGameObject = NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact.gameObject;
                }
                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("select"))
                {
                    Selection.activeGameObject = NimbatMirrorData.mirrorContactsList[i].vrcObject_NoMirror.contact.gameObject;
                }
            }


            GUILayout.EndHorizontal();
        }



       GUILayout.EndScrollView();
        

    }

    public override void OnEnable()
    {
        RefreshVRCObjectData();
    }

    #endregion

    /// <summary>
    /// Triggers a refresh of all the data, gets all vrc components and sorts them in lists to be displayed
    /// correctly
    /// </summary>
    public void RefreshVRCObjectData()
    {
        FindContacts();
        SortContactsToMirrorData();
    }

    void FindContacts()
    {
        NimbatMirrorData.avatarContacts = new List<ContactBase>(Editor.FindObjectsOfType<ContactBase>());
    }

    void SortContactsToMirrorData()
    {
        NimbatMirrorData.mirrorContactsList = new List<NimbatMirrorObject>();

        NimbatMirrorObject tempMirrorObject;        
        string tempMirrorContactName;
        bool isRightSide;

        for (int i = 0; i < NimbatMirrorData.avatarContacts.Count; i++)
        {
            //--we only do operations if the space is not null
            if (NimbatMirrorData.avatarContacts[i] != null)
            {
                if (NimbatFunctions.NameHasMirrorSuffix(NimbatMirrorData.avatarContacts[i].name, out isRightSide))
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.groupName = NimbatFunctions.MirrorNameToNoSuffix(NimbatMirrorData.avatarContacts[i].name);

                    tempMirrorContactName = NimbatFunctions.MirrorNameSuffix(NimbatMirrorData.avatarContacts[i].name);
                    

                    if (isRightSide)
                    {
                        tempMirrorObject.vrcObject_Right.contact = NimbatMirrorData.avatarContacts[i];
                    }
                    else
                    {
                        tempMirrorObject.vrcObject_Left.contact = NimbatMirrorData.avatarContacts[i];
                    }

                    NimbatMirrorData.avatarContacts[i] = null;

                    for (int j = 0; j < NimbatMirrorData.avatarContacts.Count; j++)
                    {
                        if (NimbatMirrorData.avatarContacts[j] != null)
                            if (NimbatMirrorData.avatarContacts[j].name == tempMirrorContactName)
                            {
                                if (isRightSide)
                                {
                                    tempMirrorObject.vrcObject_Left.contact = NimbatMirrorData.avatarContacts[j];
                                }
                                else
                                {
                                    tempMirrorObject.vrcObject_Right.contact = NimbatMirrorData.avatarContacts[j];
                                }

                                NimbatMirrorData.avatarContacts[j] = null;
                            }
                    }

                    if (tempMirrorObject.vrcObject_Left.contact)
                    {
                        tempMirrorObject.vrcObject_Left.absoluteScale = NimbatFunctions.GetAbsoluteScale(tempMirrorObject.vrcObject_Left.contact.gameObject);
                    }
                    if (tempMirrorObject.vrcObject_Right.contact)
                    {
                        tempMirrorObject.vrcObject_Right.absoluteScale = NimbatFunctions.GetAbsoluteScale(tempMirrorObject.vrcObject_Right.contact.gameObject);
                    }

                    tempMirrorObject.mirrorGroupValid = true;

                    NimbatMirrorData.mirrorContactsList.Add(tempMirrorObject);
                }
                else
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.SetContact(NimbatMirrorData.avatarContacts[i]);

                    NimbatMirrorData.mirrorContactsList.Add(tempMirrorObject);

                    NimbatMirrorData.avatarContacts[i] = null;
                }
            }
        }
    }

    void DrawMirroredObjects()
    {
        //Dan said this line was important, this is to prevent an unity bug where handles somehow
        //do not draw if you dont do this first?
        Handles.Label(Vector3.zero,"");

        if (NimbatMirrorData.mirrorContactsList == null)
        {
            return;
        }

        foreach (NimbatMirrorObject mirrorObject in NimbatMirrorData.mirrorContactsList)
        {
            if (!mirrorObject.showInScene)
            {
                continue;
            }

            if (mirrorObject.vrcObject_Left.contact)
            {
                DrawContactHandle(mirrorObject.vrcObject_Left, Color.blue);
            }

            if (mirrorObject.vrcObject_Right.contact)
            {
                DrawContactHandle(mirrorObject.vrcObject_Right, Color.red);
            }
        }
    }

    void DrawContactHandle(NimbatVRCObjectBase vrcObject, Color color)
    {
        Color defaultGUIColor = Handles.color;

        if (Selection.activeGameObject == vrcObject.contact.gameObject)
        {
            return;
        }

        Handles.color = color;
        
        if(vrcObject.contact.shapeType == ContactBase.ShapeType.Sphere)
        {
            HandlesUtil.DrawWireSphere(NimbatFunctions.GetContactPosition(vrcObject.contact), vrcObject.vrcObjectRadius);
        }
        else
        {
            HandlesUtil.DrawWireCapsule(vrcObject.getPosition, vrcObject.getRotation, vrcObject.contact.height * vrcObject.absoluteScale, vrcObject.vrcObjectRadius);
        }

        if (!NimbatCore.ctrlDown)
        {
            return;
        }


        if (Handles.Button(
            NimbatFunctions.GetContactPosition(vrcObject.contact),
            Quaternion.identity,
            .01f,
            vrcObject.contact.radius * vrcObject.absoluteScale,
            Handles.SphereHandleCap))
        {
            Selection.activeGameObject = vrcObject.contact.gameObject;
        }


        Handles.color = defaultGUIColor;
    }

    public GameObject GetMirrorFromGameObject(GameObject targetObject)
    {
        //--keep an eye on this in case it is slow

        if(NimbatMirrorData.mirrorContactsList == null || NimbatMirrorData.mirrorContactsList.Count <= 0)
        {
            RefreshVRCObjectData();
        }

        for (int i = 0; i< NimbatMirrorData.mirrorContactsList.Count; i++)
        {
            if(NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact.gameObject == targetObject)
            {
                return NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact.gameObject;
            }
            if (NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact.gameObject == targetObject)
            {
                return NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact.gameObject;
            }
        }
        return null;
    }

    public NimbatMirrorObject GetMirrorObjectDataFromGameObject(GameObject targetObject)
    {
        if (NimbatMirrorData.mirrorContactsList == null || NimbatMirrorData.mirrorContactsList.Count <= 0)
        {
            RefreshVRCObjectData();
        }

        for (int i = 0; i < NimbatMirrorData.mirrorContactsList.Count; i++)
        {
            if (NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact)
                if (NimbatMirrorData.mirrorContactsList[i].vrcObject_Left.contact.gameObject == targetObject)
                {
                    return NimbatMirrorData.mirrorContactsList[i];
                }
            if(NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact)
                if (NimbatMirrorData.mirrorContactsList[i].vrcObject_Right.contact.gameObject == targetObject)
                {
                    return NimbatMirrorData.mirrorContactsList[i];
                }
        }
        return null;
    }
}