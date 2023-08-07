using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

/*
public class Editor_NimbatToolsOLD : EditorWindow
{

    static  ContactBase[] contacts;
    static List<ContactBase> avatarContacts;
    static List<NimbatMirrorObject> mirrorObjects;

    static NimbatScriptableObject nimbatSO;
    static bool savePlaymodeChanges;

    static ContactBase activeContact;
    static ContactBase previousContact;    //--used to know if we selected a different contact

    static SelectedVRCObjectData_OLD selectedContact;
    static NimbatToolsSettings settings;

    static Color defaultGUIColor;
    static Color noColor = new Color(1, 1, 1, 0);

    static float physBoneTestSlider = .5f;
    static float keyID = 0;

    static VRCPhysBoneBase tempPhysBone;

    /// <summary>
    /// On gui is responsible of drawing the ui in the custom inspector window
    /// </summary>
    private void OnGUI()
    {
        Color guiDefColor = GUI.color;

        if (savePlaymodeChanges)
        {
            GUI.color = Color.green;
        }

        settings.realTimePositionMirror = GUILayout.Toggle(settings.realTimePositionMirror, "real time position mirror");
        settings.realTimeRadiusMirror = GUILayout.Toggle(settings.realTimeRadiusMirror, "real time radius mirror");

        if (GUILayout.Button("Reset transform to prefab"))
        {
            NimbatFunctions.ResetTransformToPrefabDefault(Selection.activeGameObject);
        }

        if (GUILayout.Button("Save Playmode changes"))
        {
            if (savePlaymodeChanges)
                savePlaymodeChanges = false;
            else
                savePlaymodeChanges = true;
        }

        GUI.color = guiDefColor;

        GUILayout.Label("Last selected contact data");
        GUILayout.Label("absolute scale " + selectedContact.absoluteScale.ToString());

        if (selectedContact.mirrorContact)
        {
            if (GUILayout.Button("mirror to " + selectedContact.mirrorContact.gameObject.name))
            {
                MirrorPosition();
            }
        }

        if (GUILayout.Button("get parent gameobject scales"))
        {
            NimbatFunctions.GetAbsoluteScale(Selection.activeGameObject);
        }

        if (GUILayout.Button("Find All Contacts"))
        {
            FindContacts();
        }

        if(GUILayout.Button("Sort contact data"))
        {
            SortContactsToMirrorData();
        }

        physBoneTestSlider = GUILayout.HorizontalSlider(physBoneTestSlider, 0, 1);
        
    }

    static void SceneViewGUI()
    {
        defaultGUIColor = GUI.color;
        /*
        GUILayout.Label("============ mirrored objects ===============");

        
        for (int i = 0; i < mirrorObjects.Count; i++)        
        {
            DrawMirroredObjectGUI(mirrorObjects[i]);
        }
    }
    
    static void DrawMirroredObjectGUI(NimbatMirrorObject mirrorObject)
    {
        GUILayout.BeginHorizontal();

        if (mirrorObject.showInScene)
        {
            GUI.color = defaultGUIColor;
        }
        else
        {
            GUI.color = Color.black;
        }

        if (GUILayout.Button("show"))
        {
            mirrorObject.showInScene = !mirrorObject.showInScene;
        }

        GUI.color = defaultGUIColor;

        GUILayout.Label(mirrorObject.objectName_NoSuffix);

        if (mirrorObject.contact_Left.contact != null)
        {
            if (GUILayout.Button("_L"))
            {
                Selection.activeGameObject = mirrorObject.contact_Left.contact.gameObject;
            }
        }
        else
        {
            GUILayout.Label("_L");
        }
        GUILayout.Label("  ||  ");
        if (mirrorObject.contact_Right.contact != null)
        {
            if (GUILayout.Button("_R"))
            {
                Selection.activeGameObject = mirrorObject.contact_Right.contact.gameObject;
            }
        }
        else
        {
            GUILayout.Label("_R");
        }

        GUILayout.EndHorizontal();
    }


    /// <summary>
    /// On Scene gui is the functi
    /// handles in the scene view</summary>
    /// <param name="sceneView"></param>
    static private void OnSceneGUI(SceneView sceneView)
    {
        Handles.BeginGUI();

        // Draw your additional GUI elements here
        SceneViewGUI();

        Handles.EndGUI();

        DrawMirroredObjects();

        //frist we check if we have a selected game object
        if (Selection.activeGameObject)
        {
            //we try to get an active contact component from the selected gameobject
            activeContact = Selection.activeGameObject.GetComponent<ContactBase>();


            if(activeContact != previousContact)
            {
                UpdateSelectedContact();
            }
        }
        else
        {
            activeContact = null;
        }


        //DrawSelectedColliderHandles();

        //DrawSelectedPhysboneHandles();

        if (Application.isPlaying)
        {

            if (!nimbatSO)
            {
                GenerateTempFile();
            }

            if (activeContact && activeContact.gameObject)
            {                
                nimbatSO.gameObjectName = activeContact.gameObject.name;

                nimbatSO.position = activeContact.transform.position;
                nimbatSO.contactRadius = activeContact.radius;
            }
            else
            {
                activeContact = null;
                nimbatSO.gameObjectName = "";
            }
        }
        else
        {
            if(contacts == null || contacts.Length <= 0)
            {
                return;
            }

            for (int  i = 0; i< contacts.Length; i++)
            {
                if(contacts[i].transform.position.x > .01f)
                {
                    Handles.color = new Color(1, 0, 0, .5f);
                }
                else if (contacts[i].transform.position.x < -.01f)
                {
                    Handles.color = new Color(0,0,1,.5f);
                
                }
                else
                {
                    Handles.color = new Color(0, 1, 0, .5f);
                }

                //HandlesUtil.DrawWireSphere(contacts[i].transform.position, contacts[i].radius*10);
                Handles.DrawSphere(0, contacts[i].transform.position, Quaternion.Euler(Vector3.zero), contacts[i].radius * 24);
            }
        }
    }

    static private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredPlayMode:

                EnterPlayMode();

                break;

            case PlayModeStateChange.EnteredEditMode:

                ExitPlayMode();

                break;
        }
    }

    static void DrawMirroredObjects()
    {
        if(mirrorObjects == null)
        {
            return;
        }

        foreach(NimbatMirrorObject mirrorObject in mirrorObjects)
        {
            if (!mirrorObject.showInScene)
            {
                continue;
            }
            
            if (mirrorObject.contact_Left.contact)
            {
                DrawContactHandle(mirrorObject.contact_Left, Color.blue);
            }
            
            if (mirrorObject.contact_Right.contact)
            {
                DrawContactHandle(mirrorObject.contact_Right, Color.red);
            }
        }
    }

    static void DrawContactHandle(NimbatVRCObjectBase vrcObject, Color color)
    {
        if(Selection.activeGameObject == vrcObject.contact.gameObject)
        {
            return;
        }

        Handles.color = color;

        HandlesUtil.DrawWireSphere(vrcObject.contact.transform.position, vrcObject.contact.radius * vrcObject.absoluteScale);

        

        Handles.color = noColor;

        if (Handles.Button(
            vrcObject.contact.transform.position,
            Quaternion.identity,
            .01f,
            vrcObject.contact.radius * vrcObject.absoluteScale,
            Handles.SphereHandleCap))
        {
            Selection.activeGameObject = vrcObject.contact.gameObject;
        }


        Handles.color = defaultGUIColor;
    }

    static void EnterPlayMode()
    {
        GenerateTempFile();           
    }

    static void DrawSelectedColliderHandles()
    {
        if (activeContact)
        {
            #region ====================== Radius mirroring


            EditorGUI.BeginChangeCheck();
            selectedContact.absoluteScale = NimbatFunctions.GetAbsoluteScale(Selection.activeGameObject);

            //draws the contact radius handle
            float newRadius = Handles.RadiusHandle(
                Quaternion.identity,
                activeContact.transform.position,
                activeContact.radius * selectedContact.absoluteScale,
                true);


            if (EditorGUI.EndChangeCheck())
            {
                activeContact.radius = newRadius / selectedContact.absoluteScale;

                if (settings.realTimeRadiusMirror)
                    if (selectedContact.mirrorContact)
                    {
                        selectedContact.mirrorContact.radius = activeContact.radius;
                    }
            }

            Handles.color = new Color(0, 1, 0, .3f);
            Handles.SphereHandleCap(1,
                    activeContact.transform.position,
                    Quaternion.identity,
                    (activeContact.radius * 2) * selectedContact.absoluteScale
                    , EventType.Repaint);


            //draws the mirrored contact
            if (selectedContact.mirrorContact)
            {
                if (selectedContact.mirrorType == MirrorTypes.Right)
                {
                    Handles.color = new Color(1, 0, 0, .3f);
                }
                if (selectedContact.mirrorType == MirrorTypes.Left)
                {
                    Handles.color = new Color(0, 0, 1, .3f);
                }

                Handles.SphereHandleCap(1,
                    selectedContact.mirrorContact.transform.position,
                    Quaternion.identity,
                    (selectedContact.mirrorContact.radius * 2) * selectedContact.absoluteScale
                    , EventType.Repaint);
            }

            #endregion

            if (settings.realTimePositionMirror)
                if (activeContact.transform.hasChanged)
                    if (selectedContact.mirrorContact)
                    {
                        MirrorPosition();
                        activeContact.transform.hasChanged = false;
                    }

        }

    }

    static void DrawSelectedPhysboneHandles()
    {
        

        if (Selection.activeGameObject)
        {
            tempPhysBone = Selection.activeGameObject.GetComponent<VRCPhysBoneBase>();            
        }
        else
        {
            tempPhysBone = null;
        }

        if (tempPhysBone)
        {
            Transform rootTransform;

            if(tempPhysBone.rootTransform.childCount > 1)
            {
                rootTransform = tempPhysBone.rootTransform.GetChild(0);
            }
            else
            {
                rootTransform = tempPhysBone.rootTransform;
            }

            NimbatPhysBoneDrawer.UpdateChain(rootTransform);

            
            foreach(Transform physBoneTransform in NimbatPhysBoneDrawer.physBoneTransform)
            {
                NimbatPhysBoneDrawer.DrawTestPhysboneOrientAxes(physBoneTransform);
            }            


            /*
            foreach (NimbatPhysBoneSegment physSegment in NimbatPhysBoneDrawer.physBoneSegments)
            {
                DrawTestPhysboneSegment(physSegment);
            }
            -

            AnimationCurve tempCurve = tempPhysBone.radiusCurve;

            for (int i = 0; i< tempCurve.keys.Length; i++)
            {
                float keyValue = tempCurve.keys[i].value;
                Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(tempCurve.keys[i].time);
                float radiusScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(tempCurve.keys[i].time) * keyValue) * tempPhysBone.radius;

                Handles.color = Color.red;
                HandlesUtil.DrawWireSphere(keyPosition, radiusScale);
                
            }           

            DrawCurvePointHandles( (int) keyID);


            //Handles.SphereHandleCap(1, NimbatPhysBoneDrawer.GetPosition(physBoneTestSlider), Quaternion.identity, .1f, EventType.Repaint);
        }
    }

    static void DrawCurvePointHandles(int id)
    {

        id = 2;

        AnimationCurve tempCurve = tempPhysBone.radiusCurve;

        float keyValue = tempCurve.keys[id].value;
        Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(tempCurve.keys[id].time);
        float absoluteScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(tempCurve.keys[id].time));
        float scaledValue = keyValue * tempPhysBone.radius;
        

        Handles.color = Color.green;        
        
        
        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            keyPosition,
            scaledValue * absoluteScale,
            true);

        Keyframe newKey = new Keyframe();
        newKey.value = ((newRadius / tempPhysBone.radius) / absoluteScale);
        newKey.time = tempCurve.keys[id].time;

        tempPhysBone.radiusCurve.MoveKey(id, newKey);
    }

    static void DrawTestPhysboneSegment(NimbatPhysBoneSegment physboneSegment)
    {
        Color defaultColor = Handles.color;
        Handles.color = Color.blue;
        float axisScale = .05f;
        Handles.DrawLine(physboneSegment.segmentStart.position, physboneSegment.segmentEnd.position);        
        Handles.color = defaultColor;

        //Handles.Label(physboneSegment.segmentStart.position, physboneSegment.distanceAtStart.ToString());

        Handles.Label(physboneSegment.segmentEnd.position, physboneSegment.distanceAtEnd.ToString());
    }




    static void GenerateTempFile()
    {
        nimbatSO = ScriptableObject.CreateInstance<NimbatScriptableObject>();

        
        AssetDatabase.CreateAsset(nimbatSO, NimbatData.tempPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void ExitPlayMode() 
    {
        if (savePlaymodeChanges)
        {
            GameObject targetObject = GameObject.Find(nimbatSO.gameObjectName);
            ContactBase targetContact = targetObject.GetComponent<ContactBase>();

            targetObject.transform.position = nimbatSO.position;
            targetContact.radius = nimbatSO.contactRadius;
        }

        AssetDatabase.DeleteAsset(NimbatData.tempPath);
        savePlaymodeChanges = false;
        nimbatSO = null;
    }

    static void UpdateSelectedContact()
    {

        //we use this to know if we have selected a new contact 
        if (!activeContact)
        {
            previousContact = null;
            return;
        }

        //we read the gameobject name to know if we should look for a mirror gameobject and tag
        //it to the proper side
        if (activeContact.gameObject.name.Contains("_L"))
        {
            selectedContact.mirrorType = MirrorTypes.Left;
        }
        if (activeContact.gameObject.name.Contains("_R"))
        {
            selectedContact.mirrorType = MirrorTypes.Right;
        }

        selectedContact.currentContact = activeContact;

        selectedContact.absoluteScale = NimbatFunctions.GetAbsoluteScale(activeContact.gameObject);

        string mirrorName = activeContact.gameObject.name.Substring(0, activeContact.gameObject.name.Length-2) ;

        switch (selectedContact.mirrorType)
        {
            case MirrorTypes.Left:
                mirrorName += "_R";
                break;
            case MirrorTypes.Right:
                mirrorName += "_L";
                break;
        }

        GameObject mirrorContactGO = GameObject.Find(mirrorName);

        if(mirrorContactGO != null)
        {
            selectedContact.mirrorContact = mirrorContactGO.GetComponent<ContactBase>();
        }
        else
        {
            selectedContact.mirrorContact = null;
        }

        previousContact = activeContact;
    }

    static void MirrorPosition()
    {
        Vector3 mirrorPosition = selectedContact.currentContact.transform.localPosition;
        mirrorPosition.x *= -1;

        selectedContact.mirrorContact.transform.localPosition = mirrorPosition;
        selectedContact.mirrorContact.radius = selectedContact.currentContact.radius;
    }

    static void FindContacts()
    {
        avatarContacts = new List<ContactBase>(FindObjectsOfType<ContactBase>());


    }

    static void SortContactsToMirrorData()
    {
        mirrorObjects = new List<NimbatMirrorObject>();

        NimbatMirrorObject tempMirrorObject;
        ContactBase tempContact;
        string tempMirrorContactName;
        bool isRightSide;

        for (int i = 0; i < avatarContacts.Count; i++)
        {
            //--we only do operations if the space is not null
            if(avatarContacts[i] != null)
            {
                if(NimbatFunctions.NameHasMirrorSuffix(avatarContacts[i].name, out isRightSide))
                {

                    tempMirrorObject = new NimbatMirrorObject();
                    tempMirrorObject.objectName_NoSuffix = NimbatFunctions.MirrorNameToNoSuffix(avatarContacts[i].name);

                    tempMirrorContactName = NimbatFunctions.MirrorNameSuffix(avatarContacts[i].name);

                    //Debug.Log("found mirror object with name " + avatarContacts[i].name + " || mirror name is " + tempMirrorContactName );

                    if (isRightSide)
                    {
                        tempMirrorObject.contact_Right.contact = avatarContacts[i];                        
                    }
                    else
                    {
                        tempMirrorObject.contact_Left.contact = avatarContacts[i];
                    }

                    avatarContacts[i] = null;
                
                    for (int j = 0; j < avatarContacts.Count; j++)
                    {
                        if (avatarContacts[j] != null)
                            if(avatarContacts[j].name == tempMirrorContactName)
                            {
                                if (isRightSide)
                                {
                                    tempMirrorObject.contact_Left.contact = avatarContacts[j];
                                }
                                else
                                {
                                    tempMirrorObject.contact_Right.contact = avatarContacts[j];
                                }

                                avatarContacts[j] = null;
                            }
                    }

                    tempMirrorObject.contact_Left.absoluteScale = NimbatFunctions.GetAbsoluteScale(tempMirrorObject.contact_Left.contact.gameObject);
                    tempMirrorObject.contact_Right.absoluteScale = NimbatFunctions.GetAbsoluteScale(tempMirrorObject.contact_Right.contact.gameObject);

                    mirrorObjects.Add(tempMirrorObject);
                }
            }
        }
    }

    #region =========== Monobehaviour, create/destroy
    [MenuItem("Nimbat/Nimbat Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<Editor_NimbatToolsOLD>("Nimbat tools");        
    }

    private void OnFocus()
    {
        
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    static Editor_NimbatToolsOLD()
    {
        
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;

        
        /*

        FindContacts();
        SortContactsToMirrorData();         
    }


    #endregion
}

*/