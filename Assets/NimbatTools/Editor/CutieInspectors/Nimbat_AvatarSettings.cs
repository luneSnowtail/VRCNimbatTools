using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;

public class Nimbat_AvatarSettings : NimbatCutieInspectorWindow
{
    public delegate void NewAvatarSelected();
    static public NewAvatarSelected OnNewAvatarSelected;

    List<string> avatarList;
    VRCAvatarDescriptor[] avatars;
    int selectedAvatarId;
    int previousSelectedAvatar;

    public static VRCAvatarDescriptor selectedAvatar { get; private set; }
    public static Animator selectedAvatarAnimator { get; private set; }
    public static GameObject selectedAvatarGameObject { get; private set; }

    public Nimbat_AvatarSettings()
    {
        title = "Avatar Settings";
        width = NimbatData.inspectorWindowsWidth;
        height = 100;

        mainButtonIconPath = "Button_avatar";
        FindAvatars();

        NimbatCore.OnHierarchyChanged += FindAvatars;
    }

    public override void CutieInspectorContent()
    {
        if(avatars == null || avatars.Length <= 0)
        {
            FindAvatars();
        }
        GUILayout.Label("avatars found " + avatars.Length.ToString());

        selectedAvatarId = EditorGUILayout.Popup(selectedAvatarId, avatarList.ToArray());

        if(selectedAvatarId != previousSelectedAvatar)
        {
            SelectAvatarID(selectedAvatarId);

            previousSelectedAvatar = selectedAvatarId;
            OnNewAvatarSelected?.Invoke();
        }
    }

    public override void CutieInspectorHandles()
    {
        foreach(VRCAvatarDescriptor avatar in avatars)
        {
            Handles.Label(avatar.transform.position, avatar.gameObject.name);
        }
    }
    
    void FindAvatars()
    {
        avatarList = new List<string>();
        avatars = Editor.FindObjectsOfType<VRCAvatarDescriptor>();

        for(int i = 0; i < avatars.Length; i++)
        {
            avatarList.Add(avatars[i].gameObject.name);
        }

        if (!selectedAvatar)
        {
            SelectAvatarID(0);
        }
    }

    void SelectAvatarID(int id)
    {
        selectedAvatar = avatars[id];
        selectedAvatarAnimator = selectedAvatar.GetComponent<Animator>();
        selectedAvatarGameObject = selectedAvatar.gameObject;
    }
}
