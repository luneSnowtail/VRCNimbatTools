using VRC.Dynamics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NimbatData
{
    public const string tempPath = "Assets/NimbatTools/TempData.asset";

    public const int inspectorWindowsWidth = 250;
    public const int settingsWindowsWidth = 240;
}

public enum MirrorTypes
{
    Left,       //-- described object represents the left version
    Right,
    None,
}

//used to describe what type of object its selected
public enum VRCObjectType
{
    None,           //--no selected object or object selected is not vrc type (for this plugin)
    EmptyGO,        //--this is a gameobject that might be part of a mirror hierarchy but has no components
    Contact,        //--selected object is a contact (either contact or receiver)
    PhysBone
}

/// <summary>
/// the type of contact we are dealing with in vrc
/// </summary>
public enum ContactType
{
    None,
    Sender,
    Receiver
}

/// <summary>
/// This struct always contains an object of vrc type, we need to read vrcObjectType to know
/// which object this struct is selecting, vrcObjectType is set automatically after sending an 
/// object either to physbone or contact values
/// </summary>
public struct NimbatVRCObjectBase
{
    public VRCObjectType vrcObjectType { get; private set; }
    public MirrorTypes mirrorType { get; private set; }
    public ContactType contactType { get; private set; }

    public void ClearData()
    {
        vrcObjectType = VRCObjectType.None;
        mirrorType = MirrorTypes.None;
        contactType = ContactType.None;
        _baseGO = null;
        _physBone = null;
        _contact = null;
        receiver = null;
    }

    /// <summary>
    /// returns the world position plus vrc object offset
    /// </summary>
    public Vector3 getPosition
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    return _contact.transform.position + _contact.transform.TransformDirection(_contact.position);
                    
                case VRCObjectType.PhysBone:
                    return _physBone.transform.position + _physBone.transform.TransformDirection(_contact.position);
                    

            }

            return Vector3.zero;
        }
        set
        {

        }
    }

    /// <summary>
    /// returns final rotation considering both, transform and vrc object offset rotation
    /// </summary>
    public Quaternion getRotation
    {
        get
        {
            Quaternion transformRotation;
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    transformRotation = _contact.transform.rotation;
                    Quaternion vrcObjectRotation = _contact.rotation;

                    return transformRotation * vrcObjectRotation;

                    
                case VRCObjectType.PhysBone:
                    transformRotation = _physBone.transform.rotation;

                    return transformRotation;
            }

            return Quaternion.identity;
        }
        set
        {

        }
    }

    public float vrcObjectRadius
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    return _contact.radius * absoluteScale;
                    
                case VRCObjectType.PhysBone:
                    return _physBone.radius * absoluteScale;                    
            }
            return 0;
        }
        set
        {

        }
    }

    private GameObject _baseGO;
    public GameObject baseGO
    {
        get
        {
            return _baseGO;
        }
        set
        {
            if (value == null)
            {
                ClearData();
            }
            else
            {
                absoluteScale = NimbatFunctions.GetAbsoluteScale(value.gameObject);
                mirrorType = NimbatFunctions.GetNameMirrorType(value.name);
                vrcObjectType = VRCObjectType.EmptyGO;

                _physBone = null;
                _contact = null;                
            }

            baseGO = value;
        }
    }

    private VRCPhysBoneBase _physBone;
    public VRCPhysBoneBase physBone
    {
        get
        {
            return _physBone;
        }
        set
        {
            if(value == null)
            {
                ClearData();
            }
            else            
            {
                absoluteScale = NimbatFunctions.GetAbsoluteScale(value.gameObject);
                mirrorType = NimbatFunctions.GetNameMirrorType(value.name);
                vrcObjectType = VRCObjectType.PhysBone;

                _contact = null;
                _baseGO = null;
            }            
            _physBone = value;
        }
    }

    private ContactBase _contact;
    public ContactBase contact
    {
        get
        {
            return _contact;
        }
        set
        {
            if (value == null)
            {
                ClearData();
            }
            else
            {
                absoluteScale = NimbatFunctions.GetAbsoluteScale(value.gameObject);
                mirrorType = NimbatFunctions.GetNameMirrorType(value.name);
                vrcObjectType = VRCObjectType.Contact;
                
                receiver = value.GetComponent<ContactReceiver>();
                if(receiver)
                {
                    contactType = ContactType.Receiver;
                }
                else
                {
                    contactType = ContactType.Sender;
                }

                _physBone = null;                
                _baseGO = null;
            }
            _contact = value;
        }
    }
    public ContactReceiver receiver;
    

    public float absoluteScale;
}

public struct NimbatToolsSettings
{
    public bool realTimeRadiusMirror;           //if set to on, changing this radius also changes their mirrored counterpart
    public bool realTimePositionMirror;         //if set to on, moving this object also moves their mirrored counterpart
}

public struct NimbatPhysBoneLine_Point
{
    public Transform previousTransform;
    public Transform nextTransform;
    public Transform currentTransform;
    
    public float currentDistance;

    public bool isFirstPoint;
    public bool isLastPoint;

}

public struct NimbatPhysBoneSegment
{
    public Transform segmentStart;
    public Transform segmentEnd;

    public float distanceAtStart;
    public float distanceAtEnd;
}

public struct NimbatBone
{
    public Transform boneStart;
    public List<Transform> boneEnds;
}