using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;


public enum MirrorSides
{
    None,
    LeftSide,
    RightSide,
}

[System.Serializable]
public class NimbatMirrorData : ScriptableObject
{
/// return this to mirrorObjectData
    public NimbatMirrorData()
    {
        avatarContacts = new List<ContactBase>();
        vrcMirrorGroups = new List<NimbatMirrorObject>();
    }

    static public List<ContactBase> avatarContacts;
    static public List<VRCPhysBoneBase> avatarPhysbones;
    static public List<VRCPhysBoneColliderBase> avatarColliders;

    static public List<NimbatMirrorObject> vrcMirrorGroups;
}

[System.Serializable]
public class NimbatMirrorObject
{
    public VRCObjectType vrcObjectType
    {
        get
        {
            if(vrcObject_Left.vrcObjectType != VRCObjectType.None)
            {
                return vrcObject_Left.vrcObjectType;
            }
            else if(vrcObject_Right.vrcObjectType != VRCObjectType.None)
            {
                return vrcObject_Right.vrcObjectType;
            }

            return vrcObject_NoMirror.vrcObjectType;
        }
    }

    public bool mirrorGroupValid = false;
    public bool showInScene = true;

    public string groupName;                        

    public NimbatVRCObjectBase vrcObject_NoMirror;    //--contact used when mirror group is not valid

    public NimbatVRCObjectBase vrcObject_Left;
    public NimbatVRCObjectBase vrcObject_Right;

    public void SetContact(ContactBase contact)
    {
        bool isRightSide;        

        if(NimbatFunctions.NameHasMirrorSuffix(contact.name, out isRightSide))
        {
            if (isRightSide)
            {
                vrcObject_Right.contact = contact;
            }
            else
            {
                vrcObject_Left.contact = contact;
            }

            vrcObject_NoMirror.contact = null;
            mirrorGroupValid = true;

            groupName = NimbatFunctions.MirrorNameToNoSuffix(contact.name);

        }
        else
        {
            groupName = contact.name;

            vrcObject_Left.contact = null;
            vrcObject_Right.contact = null;

            vrcObject_NoMirror.contact = contact;
            mirrorGroupValid = false;
        }        
    }

}

[System.Serializable]
public struct MirrorNameRule
{
    public MirrorNameRule(CaseModes mode, RuleTypes rule, string left, string right)
    {
        caseMode = mode;
        ruleType = rule;
        leftName = left;
        rightName = right;
    }

    public enum CaseModes
    {
        CaseSensitive,
        NonCaseSensitive,
    }

    public enum RuleTypes
    {
        Contains,
        AtEnd,
    }

    public CaseModes caseMode;
    public RuleTypes ruleType;

    public string leftName;
    public string rightName;
}

public class MirrorFunctions{

    /// <summary>
    /// this function evaluates a single mirror rule and returns which side the name belongs to
    /// in function to the mirror rule settings
    /// </summary>
    /// <param name="mirrorRule">mirror rule to evaluate towards to</param>
    /// <param name="nameToEvaluate">string to evaluate</param>
    /// <returns>which side the name belongs to</returns>
    static public MirrorSides EvaluateMirrorRule(MirrorNameRule mirrorRule, string nameToEvaluate)
    {

        string leftSideString = mirrorRule.leftName;
        string rightSideString = mirrorRule.rightName;        

        int leftSideStringLght = leftSideString.Length;
        int rightSideStringLght = rightSideString.Length;

        switch (mirrorRule.ruleType)
        {
            case MirrorNameRule.RuleTypes.AtEnd:

                if(nameToEvaluate.Length < leftSideStringLght || nameToEvaluate.Length < rightSideStringLght)
                {
                    return MirrorSides.None;
                }

                string nameLeft = nameToEvaluate.Substring(nameToEvaluate.Length - leftSideStringLght);
                string nameRight = nameToEvaluate.Substring(nameToEvaluate.Length - rightSideStringLght);

                break;
            case MirrorNameRule.RuleTypes.Contains:

                break;
        }

        return MirrorSides.None;
    }
}

