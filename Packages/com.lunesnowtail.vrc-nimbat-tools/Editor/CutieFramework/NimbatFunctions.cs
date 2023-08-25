using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class NimbatFunctions
{
    /// <summary>
    /// reads all the hierarchy gameobjects and gets the scale vrc uses
    /// for the radius of the contact
    /// </summary>
    static public float GetAbsoluteScale(GameObject targetGO)
    {
        if(targetGO == null)
        {
            return 1;
        }

        if (!targetGO.transform.parent)
        {
            return 1;
        }

        List<Vector3> parentScales = new List<Vector3>();
        List<Transform> parentObjects = new List<Transform>();


        GameObject parentGO = targetGO.transform.parent.gameObject;

        float baseScale = 1;
        float largestComponent;

        parentObjects.Add(targetGO.transform);
     

        if(parentGO == null)
        {
            return 1;
        }

        while (parentGO != null)
        {
            parentScales.Add(parentGO.transform.localScale);

            parentObjects.Add(parentGO.transform);
            //Debug.Log("added gameobject " + parentGO.name +" - " + parentGO.transform.localScale);

            if (parentGO.transform.parent)
                parentGO = parentGO.transform.parent.gameObject;
            else
                parentGO = null;
        }


        for(int i = parentObjects.Count-1 ; i >= 0; i--)
        {
           
            largestComponent = Mathf.Max(
                parentObjects[i].transform.localScale.x,
                parentObjects[i].transform.localScale.y,
                parentObjects[i].transform.localScale.z);

            baseScale *= largestComponent;
        }

        //Debug.Log("absolute scale is " + baseScale.ToString());
        return baseScale;
    }

    #region ============================ Name mirroring

    /// <summary>
    /// It receives a name string and removes either _L or _R from the end of the name and returns it
    /// </summary>        
    static public string MirrorNameToNoSuffix(string objectName)
    {

        if (objectName.Contains("Left"))
        {
            return objectName.Replace("Left",string.Empty);
        }
        if (objectName.Contains("Right"))
        {
            return objectName.Replace("Right", string.Empty);
        }

        return objectName.Substring(0, objectName.Length - 2);

        
    }

    /// <summary>
    /// it changes the name suffix from _L to _R or from _R to _L to find gameobjects easier 
    /// </summary>        
    static public string MirrorNameSuffix(string objectName)
    {
        bool isRightSide = false;
        bool fullWord = false;
        string mirrorName;

        if(objectName.Contains("Left"))
        {
            fullWord = true;
            isRightSide = false;
        }
        if (objectName.Contains("Right"))
        {
            fullWord = true;
            isRightSide = true;
        }

        if (fullWord)
        {
            if (isRightSide)
            {
                return objectName.Replace("Right", "Left");
            }
            else
            {
                return objectName.Replace("Left", "Right");
            }
        }
        else
        {
            if (NameHasMirrorSuffix(objectName, out isRightSide))
            {
                mirrorName = MirrorNameToNoSuffix(objectName);

                if (isRightSide)
                {
                    mirrorName += "_L";
                }
                else
                {
                    mirrorName += "_R";
                }
                return mirrorName;
            }
        }

        return objectName;
    }

    /// <summary>
    /// Receives and object name and returns true if it has a name suffix for either _L or _R,
    /// the out bool lets us know which side it found
    /// </summary>        
    static public bool NameHasMirrorSuffix(string objectName, out bool isRight)
    {

        //--check for full left or right words here
        if (objectName.ToLower().Contains("left"))
        {
            isRight = false;
            return true;
        }
        else if (objectName.ToLower().Contains("right"))
        {
            isRight = true;
            return true;
        }

        //--make sure string is not null or empty
        if(objectName.Length < 2 || objectName == null)
        {
            isRight = false;
            return false;
        }

        //--look for _L or _R at the end
        string suffix = objectName.Substring(objectName.Length - 2);

        if (suffix.Contains("_L"))
        {
            isRight = false;
            return true;
        }

        if (suffix.Contains("_R"))
        {
            isRight = true;
            return true;
        }

        isRight = false;
        return false;
    }

    /// <summary>
    /// Returns the side of where a object with the input name belongs in Mirrortype enum
    /// </summary>    
    static public MirrorTypes GetNameMirrorType(string objectName)
    {
        //--check for full left or right words here
        if (objectName.ToLower().Contains("left"))
        {
            return MirrorTypes.Left;
        }
        else if (objectName.ToLower().Contains("right"))
        {
            return MirrorTypes.Right;
        }

        if(objectName.Length <= 2)
        {
            return MirrorTypes.None;
        }

        //--check for _L or _R
        string suffix = objectName.Substring(objectName.Length - 2);

        if (suffix.Contains("_L"))
        {
            return MirrorTypes.Left;
        }

        if (suffix.Contains("_R"))
        {
            return MirrorTypes.Right;
        }

        return MirrorTypes.None;
    }

    #endregion

    /// <summary>
    /// rotates a vector in the defined axis by the defined magnitude
    /// </summary>        
    static public Vector3 RotateVectorOnAxis(Vector3 vectorToRotate, Vector3 axis, float magnitude)
    {
        
        Vector3 vector = Quaternion.AngleAxis(magnitude,axis) * vectorToRotate;

        return vector;
    }

    /// <summary>
    /// receives a contactBase object and returns the position where the handles should be drawn
    /// considering ofset and local transform edits
    /// </summary>    
    static public Vector3 GetContactPosition(ContactBase contact)
    {
        if (!contact)
        {
            return Vector3.zero;
        }

        Vector3 handlePosition = contact.transform.position;
        Vector3 handleOffset = contact.transform.TransformDirection(contact.position);

        return handlePosition + handleOffset;
    }

    /// <summary>
    /// It receives a name string and removes either _L or _R from the end of the name and returns it
    /// </summary>        
    static public bool TagHasMirrorSuffix(string tagName, out bool isRightSide)
    {
        string lastCharacter = tagName.Substring(tagName.Length - 1);

        if(lastCharacter.Contains("L"))
        {
            isRightSide = false;
            return true;
        }
        else if (lastCharacter.Contains("R"))
        {
            isRightSide = true;
            return true;
        }

        isRightSide = false;
        return false;
    }

    /// <summary>
    /// if tag ends with "L" or "R" it returns the name with the opposite leter, if
    /// it does not has that suffix it just returns normal name
    /// </summary>    
    static public string GetTagMirrorSuffix(string tagName)
    {
        bool isRightSide;

        if(!TagHasMirrorSuffix(tagName, out isRightSide))
        {
            return tagName;
        }

        string noSuffixTag = TagToNoSuffix(tagName);

        if (isRightSide)
        {
            return noSuffixTag + "L";
        }
        else
        {
            return noSuffixTag + "R";
        }
    }

    /// <summary>
    /// It receives a local position and just flips the x axis, but, its done automatically uwu
    /// </summary>
    static public Vector3 MirrorLocalPosition(Vector3 position)
    {
        Vector3 mirroredPosition = position;
        mirroredPosition.x *= -1;

        return mirroredPosition;
    }

    static public Quaternion MirrorRotation(Quaternion rotation)
    {
        Vector3 mirrorRotation;

        mirrorRotation = rotation.eulerAngles;
        mirrorRotation.z *= -1;
        mirrorRotation.y *= -1;

        return Quaternion.Euler(mirrorRotation);
    }

    /// <summary>
    /// Makes one transform match the target transform but mirrored (in local data)
    /// </summary>
    /// <param name="currentTransform">the transform we want to mirror</param>
    /// <param name="referenceTransform">the reference transform we are copying data from</param>
    static public void MirrorTransforms(Transform referenceTransform, out Vector3 mirrorPosition, out Vector3 mirrorRotation)
    {
        mirrorPosition = referenceTransform.localPosition;
        mirrorPosition.x *= -1;

        
        mirrorRotation = referenceTransform.localRotation.eulerAngles;
        mirrorRotation.z *= -1;
        mirrorRotation.y *= -1;

    }

    /// <summary>
    /// receives a string and if it ends with "L" or "R" it removes that letter and returns that
    /// </summary>    
    static public string TagToNoSuffix(string tagName)
    {
        return tagName.Substring(0, tagName.Length - 1);
    }


    static public Transform GetTransformInChild(GameObject parent, string name)
    {
        Transform[] childs = parent.GetComponentsInChildren<Transform>();

        for(int i = 0; i< childs.Length; i++)
        {
            if (childs[i].name == name)
                return childs[i].transform;
        }

        try
        {
            Transform childFound = GameObject.Find(name).transform;
            return childFound;
        }
        catch
        {
            return null;
        }
        
    }


    #region ======================================= Prefab functinos

    /// <summary>
    /// receive a game object and returns true if it is part of a prefab
    /// and its connected
    /// </summary>    
    static public bool IsPrefab(GameObject targetTransform)
    {
        PrefabInstanceStatus prefabStatus;

        prefabStatus = PrefabUtility.GetPrefabInstanceStatus(targetTransform);
        if (prefabStatus == PrefabInstanceStatus.Connected)
        {
            return true;
        }

        return false;
    }


    static public void ResetPrefabPosition(Transform targetTransform)
    {
        SerializedObject serializedObject = new SerializedObject(targetTransform);
        SerializedProperty positionProperty = serializedObject.FindProperty("m_LocalPosition");        

        PrefabUtility.RevertPropertyOverride(positionProperty, InteractionMode.UserAction);        
    }

    static public void ResetPrefabRotation(Transform targetTransform)
    {
        SerializedObject serializedObject = new SerializedObject(targetTransform);
        SerializedProperty rotationProperty = serializedObject.FindProperty("m_LocalRotation");        

        PrefabUtility.RevertPropertyOverride(rotationProperty, InteractionMode.UserAction);        
    }

    static public void ResetPrefabScale(Transform targetTransform)
    {
        SerializedObject serializedObject = new SerializedObject(targetTransform);
        SerializedProperty scaleProperty = serializedObject.FindProperty("m_LocalScale");

        PrefabUtility.RevertPropertyOverride(scaleProperty, InteractionMode.UserAction);
    }

    #endregion
}
