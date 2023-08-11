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
    PhysBone,       //--selected object is a physbone
    Collider,       //--selected object is a physbone collider
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

        _collider = null;
        _gameObject = null;
        _physBone = null;
        _contact = null;

        receiver = null;
    }
    public void UpdateVRCData(Transform transform)
    {
        absoluteScale = NimbatFunctions.GetAbsoluteScale(transform.gameObject);
        mirrorType = NimbatFunctions.GetNameMirrorType(transform.name);
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
                    return _contact.transform.position + _contact.transform.TransformDirection(_contact.position * absoluteScale);                    
                case VRCObjectType.PhysBone:
                    return _physBone.transform.position;
                case VRCObjectType.Collider:
                    return _collider.transform.position + _collider.transform.TransformDirection(_collider.position * absoluteScale);
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
            Quaternion vrcObjectRotation;

            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    transformRotation = _contact.transform.rotation;
                    vrcObjectRotation = _contact.rotation;

                    return transformRotation * vrcObjectRotation;

                    
                case VRCObjectType.PhysBone:
                    transformRotation = _physBone.transform.rotation;

                    return transformRotation;

                case VRCObjectType.Collider:
                    transformRotation = _collider.transform.rotation;
                    vrcObjectRotation = _collider.rotation;

                    return transformRotation * vrcObjectRotation;
            }

            return Quaternion.identity;
        }
        set
        {

        }
    }

    /// <summary>
    /// returns the radius of the nested vrc object multiplied by the absolute scale,
    /// when set it also applies the right radius divided by absolute scale to the object set here
    /// </summary>
    public float vrcRadius_Scaled
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    return _contact.radius * absoluteScale;
                    
                case VRCObjectType.PhysBone:
                    return _physBone.radius * absoluteScale;

                case VRCObjectType.Collider:
                    return _collider.radius * absoluteScale;
            }
            return 1;
        }
        set
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    _contact.radius = value / absoluteScale;
                    break;
                case VRCObjectType.PhysBone:
                    _physBone.radius = value / absoluteScale;
                    break;
                case VRCObjectType.Collider:
                    _collider.radius = value / absoluteScale;
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the name of the object set in here
    /// </summary>
    public string name
    {
        get
        {
            switch(vrcObjectType)
            {
                case VRCObjectType.Collider:
                    return _collider.name;
                case VRCObjectType.Contact:
                    return _contact.name;
                case VRCObjectType.PhysBone:
                    return _physBone.name;
                case VRCObjectType.EmptyGO:
                    return _gameObject.name;
                case VRCObjectType.None:
                    return "Empty or null";
            }
            return "Empty or null";
        }
    }

    /// <summary>
    /// returns the scale used by vrc sdk to keep this object consistent
    /// </summary>
    public float absoluteScale;

    private GameObject _gameObject;
    public GameObject gameObject
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Collider:
                    return _collider.gameObject;
                case VRCObjectType.Contact:
                    return _contact.gameObject;
                case VRCObjectType.PhysBone:
                    return _physBone.gameObject;
                case VRCObjectType.EmptyGO:
                    return _gameObject;
                case VRCObjectType.None:
                    return null;
            }
            return null;
        }
        set
        {
            ClearData();
            if (value != null)            
            {
                UpdateVRCData(value.transform);
                vrcObjectType = VRCObjectType.EmptyGO;            
            }
            _gameObject = value;
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
            ClearData();
            if (value != null)
            {
                UpdateVRCData(value.transform);
                vrcObjectType = VRCObjectType.PhysBone;
            }
            _physBone = value;
        }
    }

    public VRCPhysBoneColliderBase _collider;
    public VRCPhysBoneColliderBase collider
    {
        get
        {
            return _collider;
        }
        set
        {
            ClearData();
            if (value != null)
            {
                UpdateVRCData(value.transform);
                vrcObjectType = VRCObjectType.Collider;
            }
            _collider = value;
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
            ClearData();
            if (value != null)            
            {
                UpdateVRCData(value.transform);
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
            }
            _contact = value;
        }
    }
    public ContactReceiver receiver;
    

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