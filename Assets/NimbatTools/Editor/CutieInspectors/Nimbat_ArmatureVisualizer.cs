using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Nimbat_ArmatureVisualizer : NimbatCutieInspectorWindow
{
    float boneWidth = .005f;

    #region ============================= Constructor / Destructor
    public Nimbat_ArmatureVisualizer()
    {
        title = "Armature Visualizer";
        width = NimbatData.inspectorWindowsWidth;
        height = 60;
        
        mainButtonIconPath = "Button_armature";

        NimbatCore.OnHierarchyChanged += RefreshVRCObjectData;
    }

    ~Nimbat_ArmatureVisualizer()
    {
        NimbatCore.OnHierarchyChanged -= RefreshVRCObjectData;
    }
    #endregion

    Animator vrcAvatarAnimator;    
    Transform baseTransform;

    List<Transform> boneTransforms;
    List<Transform> humanoidBoneTransform;
    List<NimbatBone> humanoidBones;


    #region ============================== Cutie Inspector overrides
    public override void CutieInspectorContent()
    {
        GUILayout.Label("BETA");
        GUILayout.Label("future version will contain group buttons");        
    }

    public override void CutieInspectorHandles()
    {
        DrawArmatureHandles();
    }

    void RefreshVRCObjectData()
    {
        FindAvatarData();
    }
    #endregion

    void FindAvatarData()
    {
        vrcAvatarAnimator = GameObject.FindObjectOfType<Animator>();
        baseTransform = vrcAvatarAnimator.GetBoneTransform(HumanBodyBones.Hips);

        //boneTransforms = new List<Transform>();
        humanoidBoneTransform = new List<Transform>();

        humanoidBones = new List<NimbatBone>();
        
        GetHumanoidBones();
        GetAditionalBones(baseTransform);
        TransformToNimbatBones();
    }

    void GetHumanoidBones()
    {
        string[] boneNames = System.Enum.GetNames(typeof(HumanBodyBones));

        for(int i = 0; i< boneNames.Length -1 ; i++)
        {
            Transform bone = vrcAvatarAnimator.GetBoneTransform((HumanBodyBones)i);

            if (bone)
            {
                humanoidBoneTransform.Add(bone);
            }
        }       
    }

    void GetAditionalBones(Transform startBone)
    {
        Component[] components;

        if (!humanoidBoneTransform.Contains(startBone))
        {
            components = startBone.GetComponents(typeof(Component));

            if(components.Length <= 1)
            {
                humanoidBoneTransform.Add(startBone);
            }
        }
      
        for(int i = 0; i< startBone.childCount; i++)
        {
            GetAditionalBones(startBone.GetChild(i));
        }
    }

    void TransformToNimbatBones()
    {
        foreach (Transform bone in humanoidBoneTransform)
        {
            humanoidBones.Add(TransformToBone(bone, humanoidBoneTransform));
        }
    }

    void DrawArmatureHandles()
    {
        if (!baseTransform)
        {
            FindAvatarData();
        }

        Handles.Label(Vector3.zero, "armature");

        
        foreach(NimbatBone bone in humanoidBones)
        {
            if (NimbatCore.ctrlDown)
            {
                if (NimbatHandles.DrawSphereButton(bone.boneStart.position, .01f, Color.green))
                {
                    Selection.activeGameObject = bone.boneStart.gameObject;
                }
            }

            if(bone.boneStart == Selection.activeTransform)
            {
                Handles.color = Color.green;
            }
            else
            {
                Handles.color = Color.white;
            }

            NimbatHandles.DrawNimbatBone(bone, boneWidth);
        }
              
    }

    static public NimbatBone TransformToBone(Transform baseTransform, List<Transform> whiteList)
    {
        NimbatBone newBone;

        newBone.boneStart = baseTransform;
        newBone.boneEnds = new List<Transform>();

        if (baseTransform.childCount > 0)
        {
            Transform child;
            

            for (int i = 0; i < newBone.boneStart.childCount; i++)
            {
                child = newBone.boneStart.GetChild(i);

                if (whiteList.Contains(child))
                {
                    newBone.boneEnds.Add(child);
                }
            }
        }

        return newBone;
    }

}
