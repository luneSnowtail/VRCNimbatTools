using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Dynamics;




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

