using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


/// <summary>
/// This class is in charge of drawing the main menu in the scene view
/// the content of each button and menu box will be drawn by other classes
/// </summary>
[InitializeOnLoad]
public class NimbatCore : EditorWindow
{
    public delegate void HierarchyChanged();
    static public HierarchyChanged OnHierarchyChanged;

    static GameObject selection_PreviousGameObject;

    static NimbatMirrorObject selectedMirrorObjectData;

    static float buttonSize = 26;
    static float buttonMargin = -5;
    static float originMargin = 10;
    static int totalButtons = 0;
  
        
    static Rect tempRect = new Rect(originMargin, originMargin, buttonSize, buttonSize);
    static Rect nimbatRect;

    static Texture2D buttonIcon_Default;
    static Texture2D nimbatIcon;

    static public Nimbat_VRCMirrorGroups vrcMirrorGroups;
    static public Nimbat_AvatarSettings vrcAvatarSettings;
    static public Nimbat_Settings nimbatSettings;
    static public Nimbat_About nimbatAbout;

    static public Nimbat_PrefabTransformOptions nimbatOptions_transform;
    static public Nimbat_ContactOptions nimbatOptions_contact;
    static public Nimbat_PhysboneOptions nimbatOptions_physBone;

    static public Vector2 cutieInspectorPositions;
    static public Vector2 cutieInspectorStart = new Vector2(40, 10);

    static public List<NimbatCutieInspectorWindow> cutieInspectorWindows;                   //Main toolbars for the plugin, about, mirror groups, settings, etc.
    static public List<NimbatCutieInspectorWindow> cutieSelectedSettingsWindows;            //windows that shows additional settings to selected items
    
    static bool firstMenuOpen;

    static Event e;
    static public bool keyConsumed;
    static public bool ctrlDown;
    static public bool shiftDown;

    static public Color handlesDefaultColor;

    [InitializeOnLoadMethod]
    static void NimbatMenuInit()
    {
        Selection.activeGameObject = null;

        Debug.Log("Nimbat tools beta initialized correctly! uwu");

        buttonIcon_Default = Resources.Load<Texture2D>("ButtonTest");
        nimbatIcon = Resources.Load<Texture2D>("Nimbat");

        //--initialize all CutieInspector classes
        vrcAvatarSettings = new Nimbat_AvatarSettings();
        vrcMirrorGroups = new Nimbat_VRCMirrorGroups();
        nimbatSettings = new Nimbat_Settings();
        nimbatAbout = new Nimbat_About();
        
        nimbatOptions_transform = new Nimbat_PrefabTransformOptions();
        nimbatOptions_contact = new Nimbat_ContactOptions();
        nimbatOptions_physBone = new Nimbat_PhysboneOptions();

        cutieInspectorWindows = new List<NimbatCutieInspectorWindow>() { vrcAvatarSettings, vrcMirrorGroups, nimbatSettings, nimbatAbout };
        cutieSelectedSettingsWindows = new List<NimbatCutieInspectorWindow>() { nimbatOptions_transform, nimbatOptions_contact, nimbatOptions_physBone };
        
        //--suscribe to scene view
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.hierarchyChanged += HierarchyChange;
    }

    static void OnSceneGUI(SceneView scene)
    {
        handlesDefaultColor = Handles.color;

        CutieInput();

        Handles.BeginGUI();

        // Draw your additional GUI elements here
        SceneViewGUI();
                
        Handles.EndGUI();

        DrawCutieHAndles();
    }

    /// <summary>
    /// Handles the keyboard input and shorcut events
    /// </summary>
    static void CutieInput()
    {
        e = Event.current;

        ctrlDown = e.control;
        shiftDown = e.shift;

        if(e.type == EventType.KeyDown)
        {
            keyConsumed = true;
        }
        else
        {
            keyConsumed = false;
        }

        if (e.keyCode == KeyCode.Alpha2 && shiftDown && keyConsumed == false)
        {
            vrcMirrorGroups.isEnabled = !vrcMirrorGroups.isEnabled;         
        }


    }

    /// <summary>
    /// TThis is the main loop that begins drawing everything in a scene
    /// </summary>
    static void SceneViewGUI()
    {
        totalButtons = 0;

        //--this is the part that draws the main buttons and their cutie inspectors if enabled
        DrawMainCutieButtons();

        //--updates selected data to something this tool can understand
        UpdateSelectionData();

        //--if selection is compatible, we draw the additional settings here
        DrawCutieSettings();

        nimbatRect.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 80, SceneView.lastActiveSceneView.camera.pixelHeight - 80, 80, 80);
        GUI.DrawTexture(nimbatRect, nimbatIcon);


    }

    /// <summary>
    /// if an active class/cutieInspector has handles, they are called here
    /// </summary>
    static void DrawCutieHAndles()
    {
        vrcMirrorGroups.DrawCutieHandles();

        switch (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                nimbatOptions_contact.DrawCutieHandles();
                break;
            case VRCObjectType.PhysBone:
                nimbatOptions_physBone.DrawCutieHandles();
                break;
        }

        
    }

    /// <summary>
    /// when selecting something, the settings are updated here
    /// </summary>
    static void UpdateSelectionData()
    {       
        if(selection_PreviousGameObject != Selection.activeGameObject)
        {
            selection_PreviousGameObject = Selection.activeGameObject;

            if(selection_PreviousGameObject == null)
            {                
                nimbatOptions_transform.activeTransform = null;
                Nimbat_SelectionData.activeSelectedGameobject = null;
                
                return;
            }
            else
            {
                Nimbat_SelectionData.activeSelectedGameobject = Selection.activeGameObject;
                nimbatOptions_transform.activeTransform = Selection.activeGameObject.transform;               
            }                        
        }

    }

    /// <summary>
    /// this is the loop that draws the main 4 buttons
    /// </summary>
    static void DrawMainCutieButtons()
    {
        cutieInspectorPositions = cutieInspectorStart;
        firstMenuOpen = false;

        for (int i = 0; i< cutieInspectorWindows.Count; i++)
        {
            if (DrawButton(cutieInspectorWindows[i].mainButtonIcon))
            {
                cutieInspectorWindows[i].isEnabled = !cutieInspectorWindows[i].isEnabled;
            }


            if (cutieInspectorWindows[i].isEnabled)
            {
                //--sets height alligned to the first button pressed
                if (!firstMenuOpen)
                {
                    firstMenuOpen = true;
                    cutieInspectorPositions.Set(cutieInspectorPositions.x, ((buttonSize + buttonMargin) * i) + originMargin);
                }

                cutieInspectorWindows[i].position = cutieInspectorPositions;
                cutieInspectorWindows[i].DrawCutieInspectorWindow();

                cutieInspectorPositions.Set(cutieInspectorPositions.x, cutieInspectorWindows[i].heightEndMargin);
            }

        }
    }

    /// <summary>
    /// this draw the additional settings for selected objects, the windows
    /// on the bottom right part of the scene view, these should only show up when
    /// we have a component of a compatible type selected
    /// </summary>
    static void DrawCutieSettings()
    {
        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.Contact)
        {
            nimbatOptions_contact.isEnabled = true;
            nimbatOptions_contact.position.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 300, SceneView.lastActiveSceneView.camera.pixelHeight - 10);
            nimbatOptions_contact.DrawCutieInspectorWindow();
        }
        else
        {
            nimbatOptions_contact.isEnabled = false;
        }

        if(Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.PhysBone)
        {            
            nimbatOptions_physBone.isEnabled = true;
            nimbatOptions_physBone.position.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 300, SceneView.lastActiveSceneView.camera.pixelHeight - 10);
            nimbatOptions_physBone.DrawCutieInspectorWindow();
        }
        else
        {
            nimbatOptions_physBone.isEnabled = false;
        }

        nimbatOptions_transform.position.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 300, SceneView.lastActiveSceneView.camera.pixelHeight - 10);
        nimbatOptions_transform.DrawCutieInspectorWindow();
    }

    static bool DrawButton(Texture2D buttonIcon)
    {
        if(buttonIcon == null)
        {
            buttonIcon = buttonIcon_Default;
        }

        tempRect = new Rect(originMargin, originMargin + ((buttonSize + buttonMargin) * totalButtons), buttonSize, 20);
        totalButtons++;

        
        

        if (GUI.Button(tempRect, buttonIcon, EditorStyles.toolbarButton))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Triggered when hierarchy changes in unity
    /// </summary>
    static void HierarchyChange()
    {
        OnHierarchyChanged?.Invoke();
        Nimbat_SelectionData.ForceSelectionChange();
    }
}
