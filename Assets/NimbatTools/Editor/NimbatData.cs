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
    static Texture2D _icon_contact;
    static Texture2D _icon_collider;
    static Texture2D _icon_physbone;

    static Texture2D icon_contact
    {
        get
        {
            if (!_icon_contact)
            {
                _icon_contact = (Texture2D)Resources.Load("icon_contact");
            }
            return _icon_contact;
        }
    }
    static Texture2D icon_collider
    {
        get
        {
            if (!_icon_collider)
            {
                _icon_collider = (Texture2D)Resources.Load("icon_collider");
            }
            return _icon_collider;
        }
    }
    static Texture2D icon_physbone
    {
        get
        {
            if (!_icon_physbone)
            {
                _icon_physbone = (Texture2D)Resources.Load("icon_physbone");
            }
            return _icon_physbone;
        }
    }


    public VRCObjectType vrcObjectType { get; private set; }
    public MirrorTypes mirrorType { get; private set; }
    public ContactType contactType { get; private set; }
    public Texture2D iconTexture
    {
        get
        {
            switch(vrcObjectType)
            {
                case VRCObjectType.Collider:
                    return icon_collider;
                case VRCObjectType.Contact:
                    return icon_contact;
                case VRCObjectType.PhysBone:
                    return icon_physbone;
            }
            return null;
        }
    }

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
    public Vector3 positionFinal
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    if(_contact)
                        return _contact.transform.position + _contact.transform.TransformDirection(_contact.position * absoluteScale);
                    break;
                case VRCObjectType.PhysBone:
                    if (_physBone)
                    {
                        if (_physBone.rootTransform)
                    {
                        return _physBone.rootTransform.position;
                    }
                        return _physBone.transform.position;
                    }
                    break;
                case VRCObjectType.Collider:
                    if(_collider)
                        return _collider.transform.position + _collider.transform.TransformDirection(_collider.position * absoluteScale);
                    break;
            }
            if(gameObject)
                return gameObject.transform.position;
            return Vector3.zero;
        }
    }

    public Vector3 position
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    return _contact.transform.position;
                case VRCObjectType.PhysBone:
                    if (_physBone.rootTransform)
                    {
                        return _physBone.rootTransform.position;
                    }
                    return _physBone.transform.position;
                case VRCObjectType.Collider:
                    return _collider.transform.position;
            }
            if (gameObject)
                return gameObject.transform.position;
            return Vector3.zero;
        }

        set
        {
            if (gameObject)
                gameObject.transform.position = value;
        }
    }

    public Vector3 positionOffset
    {
        get
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    return _contact.transform.TransformDirection(_contact.position * absoluteScale);
                case VRCObjectType.PhysBone:
                    if (_physBone.rootTransform)
                    {
                        return _physBone.rootTransform.position;
                    }
                    return _physBone.transform.position;
                case VRCObjectType.Collider:
                    return _collider.transform.TransformDirection(_collider.position * absoluteScale);
            }
            return Vector3.zero;
        }
        set
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    _contact.position =  _contact.transform.InverseTransformDirection(value / absoluteScale);
                    break;
                case VRCObjectType.Collider:
                    _collider.position = _collider.transform.InverseTransformDirection(value / absoluteScale);
                    break;
            }
        }
    }

    /// <summary>
    /// returns final rotation considering both, transform and vrc object offset rotation
    /// </summary>
    public Quaternion rotationFinal
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

    public Quaternion rotation
    {
        get
        {
            Quaternion transformRotation;            
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    transformRotation = _contact.transform.rotation;
                    return transformRotation;
                case VRCObjectType.PhysBone:
                    transformRotation = _physBone.transform.rotation;
                    return transformRotation;
                case VRCObjectType.Collider:
                    transformRotation = _collider.transform.rotation;
                    return transformRotation;
            }

            return Quaternion.identity;
        }
        set
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    _contact.transform.rotation = value;
                    break;
                case VRCObjectType.PhysBone:
                    _physBone.transform.rotation = value;
                    break;
                case VRCObjectType.Collider:
                    _collider.transform.rotation = value;
                    break;
                case VRCObjectType.EmptyGO:
                    _gameObject.transform.rotation = value;
                    break;
            }
        }
    }

    public Quaternion rotationOffset
    {
        get
        {
            Quaternion transformRotation;
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    transformRotation = _contact.rotation;
                    return transformRotation;
                case VRCObjectType.PhysBone:                    
                    return Quaternion.identity;
                case VRCObjectType.Collider:
                    transformRotation = _collider.rotation;
                    return transformRotation;
            }

            return Quaternion.identity;
        }
        set
        {
            switch (vrcObjectType)
            {
                case VRCObjectType.Contact:
                    _contact.rotation = value;
                    break;
                case VRCObjectType.Collider:
                    _collider.rotation = value;
                    break;
                case VRCObjectType.EmptyGO:
                    _gameObject.transform.rotation = value;
                    break;
            }
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
                    if (_physBone.rootTransform) 
                        return _physBone.radius *  NimbatFunctions.GetAbsoluteScale(_physBone.rootTransform.gameObject);
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
            return _gameObject;
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
                _gameObject = value.gameObject;
            }
            else
            {
                _gameObject = null;
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
                _gameObject = value.gameObject;
            }
            else
            {
                _gameObject = null;
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
                _gameObject = value.gameObject;
            }
            else
            {
                _gameObject = null;
            }
            _contact = value;
        }
    }
    public ContactReceiver receiver;    
}

public struct NimbatPhysBoneSegment
{
    public Transform segmentStart;
    public Transform segmentEnd;

    public float segmentSize;

    public float distanceAtStart;
    public float distanceAtEnd;
}

public struct NimbatPhysBoneCurvePoint
{
    public Vector3 position;
    public Vector3 direction;

    public Vector3 forwardDirection;    

    public float radius;    

    public Vector3 directionScaled
    {
        get
        {
            return direction * radius;
        }
    }
    public Vector3 positionScaled
    {
        get
        {
            return position + directionScaled;
        }
    }
    public Vector3 positionScaledInverted
    {
        get
        {
            return position - directionScaled;
        }
    }

}
public struct NimbatBone
{
    public Transform boneStart;
    public List<Transform> boneEnds;
}