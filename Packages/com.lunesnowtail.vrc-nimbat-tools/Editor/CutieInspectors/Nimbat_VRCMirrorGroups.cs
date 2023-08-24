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

    static Texture2D icon_collider;
    static Texture2D icon_contact;
    static Texture2D icon_physbone ;

    int groupHeight = 17;

    bool tabPhysbones = true;
    bool tabContacts = true;
    bool tabColliders = true;

    bool _tabPhysbones = true;
    bool _tabContacts = true;
    bool _tabColliders = true;

    public Nimbat_VRCMirrorGroups()
    {
        title = "VRC Mirror Groups";
        width = NimbatData.inspectorWindowsWidth;
        height = 270;

        buttonIcon_visible = (Texture2D) Resources.Load("object_visible");
        buttonIcon_NotVisible = (Texture2D)Resources.Load("object_noVisible");

        icon_collider = (Texture2D)Resources.Load("icon_collider");
        icon_contact = (Texture2D)Resources.Load("icon_contact");
        icon_physbone = (Texture2D)Resources.Load("icon_physbone");
        
        mainButtonIconPath = "Button_vrcObjects";

        NimbatCore.OnHierarchyChanged += RefreshVRCObjectData;
        Nimbat_AvatarSettings.OnNewAvatarSelected += RefreshVRCObjectData;
    }

    ~Nimbat_VRCMirrorGroups()
    {
        NimbatCore.OnHierarchyChanged -= RefreshVRCObjectData;
        Nimbat_AvatarSettings.OnNewAvatarSelected -= RefreshVRCObjectData;
    }

    #region ========================== CutieInspectorOVerrides
    public override void CutieInspectorHandles()
    {
        DrawMirrorGroups();
    }

    public override void CutieInspectorContent()
    {
        GUILayout.Label("Hold CTRL and click on the center of an object to select it", EditorStyles.wordWrappedLabel);

        GUILayout.BeginHorizontal();

        tabPhysbones = GUILayout.Toggle(tabPhysbones, "Physbones", EditorStyles.toolbarButton);
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

        GUILayout.Label(string.Empty, GUILayout.Height(2));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if(NimbatMirrorData.vrcMirrorGroups == null || NimbatMirrorData.vrcMirrorGroups.Count <= 0)
        {
            GUILayout.EndScrollView();
            return;
        }

        for(int i = 0; i< NimbatMirrorData.vrcMirrorGroups.Count; i++)
        {

            switch (NimbatMirrorData.vrcMirrorGroups[i].vrcObjectType)
            {
                case VRCObjectType.Collider:
                    if (!tabColliders)
                        continue;
                    break;
                case VRCObjectType.Contact:
                    if (!tabContacts)
                        continue;
                    break;
                case VRCObjectType.PhysBone:
                    if (!tabPhysbones)
                        continue;
                    break;
            }

            GUILayout.BeginHorizontal();


            //---draws the eye to show or hide the object
            if (NimbatMirrorData.vrcMirrorGroups[i].showInScene)
            {
                NimbatMirrorData.vrcMirrorGroups[i].showInScene = GUILayout.Toggle(NimbatMirrorData.vrcMirrorGroups[i].showInScene, buttonIcon_visible, EditorStyles.toolbarButton, GUILayout.Width(20), GUILayout.Height(groupHeight));
            }
            else
            {
                NimbatMirrorData.vrcMirrorGroups[i].showInScene = GUILayout.Toggle(NimbatMirrorData.vrcMirrorGroups[i].showInScene, buttonIcon_NotVisible, EditorStyles.toolbarButton, GUILayout.Width(20), GUILayout.Height(groupHeight));
            }

            switch (NimbatMirrorData.vrcMirrorGroups[i].vrcObjectType)
            {
                case VRCObjectType.Collider:
                    GUILayout.Label(icon_collider, GUILayout.Width(18), GUILayout.Height(groupHeight));
                    break;
                case VRCObjectType.Contact:
                    GUILayout.Label(icon_contact, GUILayout.Width(18), GUILayout.Height(groupHeight));
                    break;
                case VRCObjectType.PhysBone:
                    GUILayout.Label(icon_physbone, GUILayout.Width(18), GUILayout.Height(groupHeight));
                    break;
            }
            

            //--draws the group name
            GUILayout.Label(NimbatMirrorData.vrcMirrorGroups[i].groupName, GUILayout.Width(120), GUILayout.Height(groupHeight));

            //--checks for mirror data and draws "_L" or "_R" or "Select" button
            if (NimbatMirrorData.vrcMirrorGroups[i].mirrorGroupValid)
            {
                //--before drawing the button to select an object we make sure it exist
                GUI.enabled = NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject;
                                                    
                if (GUILayout.Button("_L", GUILayout.Width(25), GUILayout.Height(groupHeight)))
                {                
                    Selection.activeGameObject = NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject;
                }
                GUI.enabled = true;

                //--before drawing the button to select an object we make sure it exist
                GUI.enabled = NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject;
                
                if (GUILayout.Button("_R", GUILayout.Width(25), GUILayout.Height(groupHeight)))
                {                
                    Selection.activeGameObject = NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject;
                }

                GUI.enabled = true;
            }
            else
            {
                if (GUILayout.Button("select", GUILayout.Width(52), GUILayout.Height(groupHeight)))
                {
                    Selection.activeGameObject = NimbatMirrorData.vrcMirrorGroups[i].vrcObject_NoMirror.gameObject;
                }
            }


            GUILayout.EndHorizontal();
        }
       GUILayout.EndScrollView();

       GUILayout.Label(string.Empty, GUILayout.Height(2));
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
        if(!Nimbat_AvatarSettings.sceneHasVRCAvatars || !Nimbat_AvatarSettings.selectedAvatar)
        {
            return;
        }

        FindVRCObjects();
        CreateMirrorGroups();
    }

    void FindVRCObjects()
    {        
        if(Nimbat_AvatarSettings.selectedAvatar == null || !Nimbat_AvatarSettings.sceneHasVRCAvatars)
        {
            return;
        }

        NimbatMirrorData.avatarContacts = new List<ContactBase>(Nimbat_AvatarSettings.selectedAvatar.gameObject.GetComponentsInChildren<ContactBase>());
        NimbatMirrorData.avatarColliders = new List<VRCPhysBoneColliderBase>(Nimbat_AvatarSettings.selectedAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneColliderBase>());
        NimbatMirrorData.avatarPhysbones = new List<VRCPhysBoneBase>(Nimbat_AvatarSettings.selectedAvatar.gameObject.GetComponentsInChildren<VRCPhysBoneBase>());
    }

    void CreateMirrorGroups()
    {
        //--create list of mirrored objects
        NimbatMirrorData.vrcMirrorGroups = new List<NimbatMirrorObject>();

        //--temporary mirror object
        

        CreateMirrorGroupsForContacts();
        CreateMirrorGroupsForPhysbones();
        CreateMirrorGroupsForColliders();
    }

    void CreateMirrorGroupsForContacts()
    {
        NimbatMirrorObject tempMirrorObject;
        string tempMirrorObjectName;
        bool isRightSide;

        for (int i = 0; i < NimbatMirrorData.avatarContacts.Count; i++)
        {
            //--we only do operations if the space is not null
            if (NimbatMirrorData.avatarContacts[i] == null)
                continue;
            
            if (NimbatFunctions.NameHasMirrorSuffix(NimbatMirrorData.avatarContacts[i].name, out isRightSide))
            {
                tempMirrorObject = new NimbatMirrorObject();
                tempMirrorObject.groupName = NimbatFunctions.MirrorNameToNoSuffix(NimbatMirrorData.avatarContacts[i].name);

                tempMirrorObjectName = NimbatFunctions.MirrorNameSuffix(NimbatMirrorData.avatarContacts[i].name);


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
                        if (NimbatMirrorData.avatarContacts[j].name == tempMirrorObjectName)
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

                tempMirrorObject.mirrorGroupValid = true;
                NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);
            }
            else
            {
                tempMirrorObject = new NimbatMirrorObject();
                tempMirrorObject.SetContact(NimbatMirrorData.avatarContacts[i]);

                NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);

                NimbatMirrorData.avatarContacts[i] = null;
            }
            
        }
    }

    void CreateMirrorGroupsForPhysbones()
    {
        NimbatMirrorObject tempMirrorObject;
        string tempMirrorObjectName;
        bool isRightSide;

        for (int i = 0; i < NimbatMirrorData.avatarPhysbones.Count; i++)
        {
            //--we only do operations if the space is not null
            if (NimbatMirrorData.avatarPhysbones[i] != null)
            {
                if (NimbatFunctions.NameHasMirrorSuffix(NimbatMirrorData.avatarPhysbones[i].name, out isRightSide))
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.groupName = NimbatFunctions.MirrorNameToNoSuffix(NimbatMirrorData.avatarPhysbones[i].name);

                    tempMirrorObjectName = NimbatFunctions.MirrorNameSuffix(NimbatMirrorData.avatarPhysbones[i].name);


                    if (isRightSide)
                    {
                        tempMirrorObject.vrcObject_Right.physBone = NimbatMirrorData.avatarPhysbones[i];
                    }
                    else
                    {
                        tempMirrorObject.vrcObject_Left.physBone = NimbatMirrorData.avatarPhysbones[i];
                    }

                    NimbatMirrorData.avatarPhysbones[i] = null;

                    for (int j = 0; j < NimbatMirrorData.avatarPhysbones.Count; j++)
                    {
                        if (NimbatMirrorData.avatarPhysbones[j] != null)
                            if (NimbatMirrorData.avatarPhysbones[j].name == tempMirrorObjectName)
                            {
                                if (isRightSide)
                                {
                                    tempMirrorObject.vrcObject_Left.physBone = NimbatMirrorData.avatarPhysbones[j];
                                }
                                else
                                {
                                    tempMirrorObject.vrcObject_Right.physBone = NimbatMirrorData.avatarPhysbones[j];
                                }

                                NimbatMirrorData.avatarPhysbones[j] = null;
                            }
                    }

                    tempMirrorObject.mirrorGroupValid = true;
                    NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);
                }
                else
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.vrcObject_NoMirror.physBone = NimbatMirrorData.avatarPhysbones[i];

                    tempMirrorObject.groupName = NimbatMirrorData.avatarPhysbones[i].name;

                    NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);

                    NimbatMirrorData.avatarPhysbones[i] = null;
                }
            }
        }
    }

    void CreateMirrorGroupsForColliders()
    {        
        NimbatMirrorObject tempMirrorObject;
        string tempMirrorObjectName;
        bool isRightSide;

        for (int i = 0; i < NimbatMirrorData.avatarColliders.Count; i++)
        {
            //--we only do operations if the space is not null
            if (NimbatMirrorData.avatarColliders[i] != null)
            {
                if (NimbatFunctions.NameHasMirrorSuffix(NimbatMirrorData.avatarColliders[i].name, out isRightSide))
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.groupName = NimbatFunctions.MirrorNameToNoSuffix(NimbatMirrorData.avatarColliders[i].name);

                    tempMirrorObjectName = NimbatFunctions.MirrorNameSuffix(NimbatMirrorData.avatarColliders[i].name);


                    if (isRightSide)
                    {
                        tempMirrorObject.vrcObject_Right.collider = NimbatMirrorData.avatarColliders[i];
                    }
                    else
                    {
                        tempMirrorObject.vrcObject_Left.collider = NimbatMirrorData.avatarColliders[i];
                    }

                    NimbatMirrorData.avatarColliders[i] = null;

                    for (int j = 0; j < NimbatMirrorData.avatarColliders.Count; j++)
                    {
                        if (NimbatMirrorData.avatarColliders[j] != null)
                            if (NimbatMirrorData.avatarColliders[j].name == tempMirrorObjectName)
                            {
                                if (isRightSide)
                                {
                                    tempMirrorObject.vrcObject_Left.collider = NimbatMirrorData.avatarColliders[j];
                                }
                                else
                                {
                                    tempMirrorObject.vrcObject_Right.collider = NimbatMirrorData.avatarColliders[j];
                                }

                                NimbatMirrorData.avatarColliders[j] = null;
                            }
                    }

                    tempMirrorObject.mirrorGroupValid = true;
                    NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);
                }
                else
                {
                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.vrcObject_NoMirror.collider = NimbatMirrorData.avatarColliders[i];

                    tempMirrorObject.groupName = NimbatMirrorData.avatarColliders[i].name;

                    NimbatMirrorData.vrcMirrorGroups.Add(tempMirrorObject);

                    NimbatMirrorData.avatarColliders[i] = null;
                }
            }
        }
    }


    void DrawMirrorGroups()
    {
        //Dan said this line was important, this is to prevent an unity bug where handles somehow
        //do not draw if you dont do this first?
        Handles.Label(Vector3.zero,string.Empty);

        if (NimbatMirrorData.vrcMirrorGroups == null)
        {
            return;
        }

        foreach (NimbatMirrorObject mirrorObject in NimbatMirrorData.vrcMirrorGroups)
        {
            if (!mirrorObject.showInScene)
            {
                continue;
            }

            if (mirrorObject.vrcObject_NoMirror.gameObject)
            {
                DrawNimbatObjectHandles(mirrorObject.vrcObject_NoMirror, Color.green);
            }

            if (mirrorObject.vrcObject_Left.gameObject)
            {                
                DrawNimbatObjectHandles(mirrorObject.vrcObject_Left, Color.blue);
            }

            if (mirrorObject.vrcObject_Right.gameObject)
            {
                DrawNimbatObjectHandles(mirrorObject.vrcObject_Right, Color.red);
            }
        }
    }

    void DrawNimbatObjectHandles(NimbatVRCObjectBase vrcObject, Color color)
    {
        Color defaultGUIColor = Handles.color;

        if (Selection.activeGameObject == vrcObject.gameObject)
        {            
            return;
        }

        //--filtering depending on objects enabled in cutie inspector tab
        switch (vrcObject.vrcObjectType)
        {
            case VRCObjectType.Collider:
                if (!tabColliders)
                    return;
                break;
            case VRCObjectType.Contact:
                if (!tabContacts)
                    return;
                break;
            case VRCObjectType.PhysBone:
                if (!tabPhysbones)
                    return;
                break;
        }

        //--draws the icon
        Handles.Label(vrcObject.positionFinal, vrcObject.iconTexture);

        Handles.color = color;

        NimbatHandles.DrawVRCNimbatObject(vrcObject);

        if (!NimbatCore.ctrlDown)
        {
            return;
        }

        if (Handles.Button(
            vrcObject.positionFinal,
            Quaternion.identity,
            .01f,
            vrcObject.vrcRadius_Scaled,
            Handles.SphereHandleCap))
        {
            Selection.activeGameObject = vrcObject.gameObject;
        }

        Handles.color = defaultGUIColor;
    }

    public GameObject GetMirrorFromGameObject(GameObject targetObject)
    {
        //--keep an eye on this in case it is slow

        if(NimbatMirrorData.vrcMirrorGroups == null || NimbatMirrorData.vrcMirrorGroups.Count <= 0)
        {
            RefreshVRCObjectData();
        }

        for (int i = 0; i< NimbatMirrorData.vrcMirrorGroups.Count; i++)
        {
            if(NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject == targetObject)
            {
                return NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject;
            }
            if (NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject == targetObject)
            {
                return NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject;
            }
        }
        return null;
    }

    public NimbatMirrorObject GetMirrorObjectDataFromGameObject(GameObject targetObject)
    {
        if (NimbatMirrorData.vrcMirrorGroups == null || NimbatMirrorData.vrcMirrorGroups.Count <= 0)
        {
            RefreshVRCObjectData();
        }

        for (int i = 0; i < NimbatMirrorData.vrcMirrorGroups.Count; i++)
        {
            if (NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject)
                if (NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Left.gameObject == targetObject)
                {
                    return NimbatMirrorData.vrcMirrorGroups[i];
                }
            if(NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject)
                if (NimbatMirrorData.vrcMirrorGroups[i].vrcObject_Right.gameObject == targetObject)
                {
                    return NimbatMirrorData.vrcMirrorGroups[i];
                }
        }
        return null;
    }

    public void SetFilterObject(VRCObjectType objectType, bool additive)
    {
        switch (objectType)
        {
            case VRCObjectType.PhysBone:
                tabPhysbones = true;
                tabContacts = false;
                tabColliders = false;

                _tabPhysbones = true;
                _tabContacts = false;
                _tabColliders = false;

                break;
            case VRCObjectType.Collider:
                tabPhysbones = false;
                tabContacts = false;
                tabColliders = true;

                _tabPhysbones = false;
                _tabContacts = false;
                _tabColliders = true;
                break;
            case VRCObjectType.Contact:
                tabPhysbones = false;
                tabContacts = true;
                tabColliders = false;

                _tabPhysbones = false;
                _tabContacts = true;
                _tabColliders = false;
                break;
        }

    }
}