using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// NimbatCutieInspectorWindow is a class that is the base for every one of the tools in the system
/// it contains an inspector window that can be easily configured and where the content can be overriden
/// to show whatever data you need to show, every tool that requires a cutieInspector window inherits from this.
/// </summary>
/// 

public enum CutieInspectorDrawModes
{
    DropDown,   //--can be expanded down
    DropUp,     //--expands but going up
}

public class NimbatCutieInspectorWindow
{
    public CutieInspectorDrawModes drawModes;

    bool _isEnabled;
    public bool isEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (value)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
            _isEnabled = value;
        }
    }                 
    public bool drawHandles = true;         //if set to true the handles are drawed in the scene view
    public bool expanded = true;            //if set to true we expand the cutie inspector window to show content

    public Vector2 position;                //position of where the window is (top left)
    public int width = 300;                 //width for the cutie inspector window
    public int height = 350;                //how long the cutie inspector is (going down)

    public string mainButtonIconPath;

    Texture2D _buttonIcon;
    public Texture2D mainButtonIcon
    {
        get
        {
            if (!_buttonIcon)
            {
                _buttonIcon =(Texture2D) Resources.Load(mainButtonIconPath);
            }
            return _buttonIcon;
        }
        set
        {

        }
    }            

    //--private variables used to draw the window
    int titleHeight = 20;       
    int margin = 5;
    public string title;
    
    public int heightEnd
    {
        get
        {
            if (expanded)
            {
                return (int) position.y + titleHeight + height;
            }

            return (int) position.y + titleHeight;
        }
        set
        {

        }
    }

    public int heightEndMargin
    {
        get
        {
            switch (drawModes)
            {
                case CutieInspectorDrawModes.DropDown:
                    if (expanded)
                    {
                        return (int)position.y + titleHeight + height + 5;
                    }

                    return (int)position.y + titleHeight + 5;
                case CutieInspectorDrawModes.DropUp:
                    if (expanded)
                    {
                        return (int)position.y - titleHeight - height;
                    }

                    return (int)position.y - titleHeight;
            }
            return 0;
        }
        set
        {

        }
    }

    Rect titleBoxRect;
    Rect titleFoldoutRect;
    Rect titleTextPositionRect;
    Rect contentBoxRect;
    Rect contentRect;


    /// <summary>
    /// Call this function in order to draw the cutie inspector window and its contents
    /// </summary>
    public void DrawCutieInspectorWindow()
    {
        if (!isEnabled)
        {
            return;
        }

        switch(drawModes)
        {
            //--rect positions when window drops down
            case CutieInspectorDrawModes.DropDown:

                titleBoxRect.Set(position.x, position.y, width, titleHeight);
                titleFoldoutRect.Set(position.x + 5, position.y, width, titleHeight);
                titleTextPositionRect.Set(position.x + 25, position.y-1, width, titleHeight);                
                contentBoxRect.Set(position.x, position.y + titleHeight - 1, width, height);
                contentRect.Set(position.x + margin, position.y + (margin *.5f)+ titleHeight, width - margin*2, height - margin);

                break;

            case CutieInspectorDrawModes.DropUp:

                titleBoxRect.Set(position.x, position.y - titleHeight, width, titleHeight);
                titleFoldoutRect.Set(position.x + 5, position.y - titleHeight, width, titleHeight);
                titleTextPositionRect.Set(position.x + 25, position.y - 1 - titleHeight, width, titleHeight);

                contentBoxRect.Set(position.x, position.y - titleHeight - height +1, width, height);
                
                contentRect.Set(position.x + margin, position.y - titleHeight - height + (margin), width - margin * 2, height - margin);


                break;
        }


        GUI.Box(titleBoxRect , "", EditorStyles.helpBox);
        expanded = EditorGUI.Foldout(titleFoldoutRect, expanded, "");
        GUI.Label(titleTextPositionRect, title, EditorStyles.boldLabel);


        if (!expanded)
            return;

        GUI.Box(contentBoxRect, "", EditorStyles.helpBox);

        GUILayout.BeginArea(contentRect);

        CutieInspectorContent();

        GUILayout.EndArea();

    }
    
    public void DrawCutieHandles()
    {
        if (isEnabled)
            CutieInspectorHandles();
    }

    virtual public void CutieInspectorHandles()
    {

    }

    virtual public void CutieInspectorContent()
    {
        GUILayout.Label("this is base content");
    }

    virtual public void OnEnable()
    {

    }

    virtual public void OnDisable()
    {

    }

    virtual public bool IsWindowValid()
    {
        return false;
    }

}
