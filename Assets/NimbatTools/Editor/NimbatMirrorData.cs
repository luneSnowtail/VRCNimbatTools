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
        mirrorContactsList = new List<NimbatMirrorObject>();
    }

    static public List<ContactBase> avatarContacts;
    static public List<NimbatMirrorObject> mirrorContactsList;
}

[System.Serializable]
public class NimbatMirrorObject
{
    public VRCObjectType vrcObjectType;

    public bool mirrorGroupValid = false;
    public bool showInScene = true;

    public string groupName;                        

    public NimbatVRCObjectBase vrcObject_NoMirror;    //--contact used when mirror group is not valid

    public NimbatVRCObjectBase vrcObject_Left;
    public NimbatVRCObjectBase vrcObject_Right;

    public void SetContact(ContactBase contact)
    {
        bool isRightSide;
        vrcObjectType = VRCObjectType.Contact; 

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

