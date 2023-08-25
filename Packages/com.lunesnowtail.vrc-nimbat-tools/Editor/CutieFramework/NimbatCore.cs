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
    static public Nimbat_ArmatureVisualizer vrcArmature;
    static public Nimbat_Settings nimbatSettings;
    static public Nimbat_About nimbatAbout;

    static public Nimbat_PrefabTransformOptions nimbatOptions_transform;
    static public Nimbat_ContactEditor nimbatOptions_contact;
    static public Nimbat_PhysboneEditor nimbatOptions_physBone;
    static public Nimbat_ColliderEditor nimbatOptions_collider;
    static public Nimbat_MirrorEditor nimbatOptions_mirror;

    static public Vector2 cutieInspectorPositions;
    static public Vector2 cutieInspectorStart = new Vector2(40, 10);

    static public Vector2 cutieInspectorEditorPositions;
    //static public Vector2 cutieInspectorEditorPositionsStart = new Vector3(SceneView.lastActiveSceneView.camera.pixelWidth - 300, SceneView.lastActiveSceneView.camera.pixelHeight - 10);

    static public List<NimbatCutieInspectorWindow> cutieInspectorWindows;                   //Main toolbars for the plugin, about, mirror groups, settings, etc.
    static public List<NimbatCutieInspectorWindow> cutieEditorWindows;            //windows that shows additional settings to selected items
    
    static bool firstMenuOpen;
    static bool firstEditorOpen;

    static Event e;
    static public bool keyConsumed { get; private set; }
    static public bool ctrlDown { get; private set; }
    static public bool shiftDown { get; private set; }
    static public bool altDown { get; private set; }
    static public bool delDown { get; private set; }

    static public bool overrideDelKey;

    static public EventType keyEventType;

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
        vrcArmature = new Nimbat_ArmatureVisualizer();
        nimbatSettings = new Nimbat_Settings();
        nimbatAbout = new Nimbat_About();
        
        nimbatOptions_transform = new Nimbat_PrefabTransformOptions();
        nimbatOptions_mirror = new Nimbat_MirrorEditor();
        nimbatOptions_contact = new Nimbat_ContactEditor();
        nimbatOptions_physBone = new Nimbat_PhysboneEditor();
        nimbatOptions_collider = new Nimbat_ColliderEditor();

        cutieInspectorWindows = new List<NimbatCutieInspectorWindow>() { vrcAvatarSettings, vrcMirrorGroups, vrcArmature ,nimbatSettings, nimbatAbout };
        cutieEditorWindows = new List<NimbatCutieInspectorWindow>() { nimbatOptions_transform, nimbatOptions_mirror, nimbatOptions_contact, nimbatOptions_physBone, nimbatOptions_collider };
        
        //--suscribe to scene view
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.hierarchyChanged += HierarchyChange;
    }

    static void OnSceneGUI(SceneView scene)
    {
        handlesDefaultColor = Handles.color;

        //--updates selected data to something this tool can understand
        UpdateSelectionData();

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
        altDown = e.alt;
        delDown = false;
        keyEventType = e.type;

        if (overrideDelKey)
        {
            if (e.keyCode == KeyCode.Delete)
            {
                e.Use();
                delDown = true;            
            }
        }

        if(e.type == EventType.KeyDown)
        {
            keyConsumed = true;
        }
        else
        {
            keyConsumed = false;
        }

        if (e.keyCode == KeyCode.Alpha1 && shiftDown && keyConsumed == false)
        {
            vrcArmature.isEnabled = !vrcArmature.isEnabled;
        }

        if (e.keyCode == KeyCode.Alpha2 && shiftDown && keyConsumed == false)
        {
            vrcMirrorGroups.isEnabled = !vrcMirrorGroups.isEnabled;
            vrcMirrorGroups.SetFilterObject(VRCObjectType.PhysBone, false);            

        }
        if (e.keyCode == KeyCode.Alpha3 && shiftDown && keyConsumed == false)
        {
            vrcMirrorGroups.isEnabled = !vrcMirrorGroups.isEnabled;
            vrcMirrorGroups.SetFilterObject(VRCObjectType.Contact, false);
        }
        if (e.keyCode == KeyCode.Alpha4 && shiftDown && keyConsumed == false)
        {
            vrcMirrorGroups.isEnabled = !vrcMirrorGroups.isEnabled;
            vrcMirrorGroups.SetFilterObject(VRCObjectType.Collider, false);
        }
                        
    }

    /// <summary>
    /// TThis is the main loop that begins drawing everything in a scene
    /// </summary>
    static void SceneViewGUI()
    {
        nimbatRect.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 80, SceneView.lastActiveSceneView.camera.pixelHeight - 80, 80, 80);
        GUI.DrawTexture(nimbatRect, nimbatIcon);

        totalButtons = 0;

        //--this is the part that draws the main buttons and their cutie inspectors if enabled
        DrawMainCutieButtons();

        //--if selection is compatible, we draw the additional settings here
        DrawCutieSettings();



    }

    /// <summary>
    /// if an active class/cutieInspector has handles, they are called here
    /// </summary>
    static void DrawCutieHAndles()
    {
        vrcMirrorGroups.DrawCutieHandles();
        vrcArmature.DrawCutieHandles();

        nimbatOptions_mirror.DrawCutieHandles();
        vrcAvatarSettings.DrawCutieHandles();

        switch (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType)
        {
            case VRCObjectType.Contact:
                nimbatOptions_contact.DrawCutieHandles();
                break;
            case VRCObjectType.PhysBone:
                nimbatOptions_physBone.DrawCutieHandles();
                break;
            case VRCObjectType.Collider:                
                nimbatOptions_collider.DrawCutieHandles();
                break;
        }        
    }

    /// <summary>
    /// when selecting something, the settings are updated here
    /// </summary>
    static void UpdateSelectionData()
    {
        if (selection_PreviousGameObject == Selection.activeGameObject)
        {
            return;
        }


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

    /// <summary>
    /// this is the loop that draws the main 4 buttons
    /// </summary>
    static void DrawMainCutieButtons()
    {
        cutieInspectorPositions = cutieInspectorStart;
        firstMenuOpen = false;

        for (int i = 0; i< cutieInspectorWindows.Count; i++)
        {
            var cutieWindow = cutieInspectorWindows[i];

            if (DrawButton(cutieWindow.mainButtonIcon))
            {
                cutieWindow.isEnabled = !cutieWindow.isEnabled;
            }


            if (cutieWindow.isEnabled)
            {
                //--sets height alligned to the first button pressed
                if (!firstMenuOpen)
                {
                    firstMenuOpen = true;
                    cutieInspectorPositions.Set(cutieInspectorPositions.x, ((buttonSize + buttonMargin) * i) + originMargin);
                }

                cutieWindow.position = cutieInspectorPositions;
                cutieWindow.DrawCutieInspectorWindow();

                cutieInspectorPositions.Set(cutieInspectorPositions.x, cutieWindow.heightEndMargin);
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
        firstEditorOpen = false;

        for(int i = 0; i< cutieEditorWindows.Count; i++)
        {
            if (!cutieEditorWindows[i].IsWindowValid())
            {
                continue;
            }
            
            if (!firstEditorOpen)
            {
                firstEditorOpen = true;
                cutieInspectorEditorPositions.Set(SceneView.lastActiveSceneView.camera.pixelWidth - 300, SceneView.lastActiveSceneView.camera.pixelHeight - 10);
            }

            cutieEditorWindows[i].isEnabled = true;
            cutieEditorWindows[i].position = cutieInspectorEditorPositions;
            cutieEditorWindows[i].DrawCutieInspectorWindow();

            cutieInspectorEditorPositions.Set(cutieInspectorEditorPositions.x, cutieEditorWindows[i].heightEndMargin);
            
        }  
    }

    static bool DrawButton(Texture2D buttonIcon)
    {
        if(buttonIcon == null)
        {
            buttonIcon = buttonIcon_Default;
        }

        tempRect = new Rect(originMargin, originMargin + ((buttonSize + buttonMargin) * totalButtons), buttonSize, 20);
        totalButtons++;

        return GUI.Button(tempRect, buttonIcon, EditorStyles.toolbarButton);
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
