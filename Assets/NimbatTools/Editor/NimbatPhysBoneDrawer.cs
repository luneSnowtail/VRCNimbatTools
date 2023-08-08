using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NimbatPhysBoneDrawer 
{
    static public List<NimbatPhysBoneLine_Point> physBoneChain;
    static public List<NimbatPhysBoneSegment> physBoneSegments;
    static public List<Transform> physBoneTransform;    
    static public float normalizedScale;

    static public float physBone_Lenght;
    static public float physBone_Radius;

    static public float segmentDistance;
    static public float segmentNormalizedDistance;

    static public float normalizedSegmentDistance;



    static public void UpdateChain(Transform targetTransform)
    {
        if (!targetTransform)
        {
            physBoneTransform = new List<Transform>();
            physBoneSegments = new List<NimbatPhysBoneSegment>();
            return;
        }

        if(targetTransform.childCount > 1)
        {
            targetTransform = targetTransform.GetChild(0);
        }

        NimbatPhysBoneSegment physBoneSegment;

        Transform currentObject = targetTransform;
        Transform childObject = targetTransform.GetChild(0);


        physBoneTransform = new List<Transform>();        
        physBoneSegments = new List<NimbatPhysBoneSegment>();

        physBoneTransform.Add(targetTransform);

        
        physBone_Lenght = 0;        

        while (childObject != null)
        {

            physBone_Lenght += Vector3.Distance(currentObject.position, childObject.position);

            physBoneTransform.Add(childObject);

            if (childObject.childCount > 0)
            {
                currentObject = childObject;
                childObject = childObject.GetChild(0);

            }
            else
            {
                childObject = null;
            }
        }
        
        segmentNormalizedDistance =  1f / (physBoneTransform.Count -1);

        float segmentTotalDistance = 0;

        for (int i = 0; i< physBoneTransform.Count; i++)
        {
            if(i < physBoneTransform.Count - 1)
            {
                physBoneSegment = new NimbatPhysBoneSegment();

                physBoneSegment.segmentStart = physBoneTransform[i];
                physBoneSegment.segmentEnd = physBoneTransform[i + 1];

                physBoneSegment.distanceAtStart = segmentTotalDistance;
                segmentTotalDistance += segmentNormalizedDistance;
                physBoneSegment.distanceAtEnd = segmentTotalDistance;

                physBoneSegments.Add(physBoneSegment);
            }

        }
        normalizedScale = physBone_Lenght / 1;
    }
    
    /// <summary>
    /// it receives a normalized float (0 to 1) and it returns the 3d position of where that is located
    /// in the physbone chain 
    /// </summary>    
    static public Vector3 GetPosition(float normalizedRange)
    {
        if(physBoneSegments.Count <= 0)
        {
            return Vector3.zero;
        }

        normalizedRange = Mathf.Clamp01(normalizedRange);

        if(normalizedRange <= 0)
        {
            return physBoneSegments[0].segmentStart.position;
        }
        else if( normalizedRange >= 1)
        {
            return physBoneSegments[physBoneSegments.Count - 1].segmentEnd.position;
        }
        
        
        for(int i = 0; i < physBoneSegments.Count; i++) 
        {
            if(normalizedRange >= physBoneSegments[i].distanceAtStart && normalizedRange <= physBoneSegments[i].distanceAtEnd)
            {
                float remainingDistance = normalizedRange - physBoneSegments[i].distanceAtStart;
                float segmentDistance = Vector3.Distance(physBoneSegments[i].segmentStart.position, physBoneSegments[i].segmentEnd.position);

                return Vector3.Lerp(physBoneSegments[i].segmentStart.position, physBoneSegments[i].segmentEnd.position, remainingDistance / segmentDistance);
            }
        }

        Debug.Log(normalizedRange.ToString());
        return Vector3.zero;
    }

    static public float GetAbsoluteScale(float normalizedRange)
    {
        if (physBoneSegments.Count <= 0)
        {
            return 1;
        }

        if (normalizedRange == 0)
        {
            return NimbatFunctions.GetAbsoluteScale(physBoneSegments[0].segmentStart.gameObject);
        }

        for (int i = 0; i < physBoneSegments.Count; i++)
        {
            if (normalizedRange > physBoneSegments[i].distanceAtStart && normalizedRange < physBoneSegments[i].distanceAtEnd)
            {
                return NimbatFunctions.GetAbsoluteScale(physBoneSegments[i].segmentStart.gameObject);
            }
        }

        return 1;
    }


    static public Vector3 GetForwardDirectionAtPosition(float normalizedRange)
    {
        if (physBoneSegments.Count <= 0)
        {
            return Vector3.zero;
        }

        for (int i = 0; i < physBoneSegments.Count; i++)
        {
            if (normalizedRange > physBoneSegments[i].distanceAtStart && normalizedRange < physBoneSegments[i].distanceAtEnd)
            {
                return physBoneSegments[i].segmentStart.TransformDirection(Vector3.up) ;
            }
        }

        return physBoneSegments[0].segmentStart.TransformDirection(Vector3.up);
    }

    static public Vector3 GetRotationAxisAtPosition(float normalizedRange)
    {
        if (physBoneSegments.Count <= 0)
        {
            return Vector3.zero;
        }

        for (int i = 0; i < physBoneSegments.Count; i++)
        {
            if (normalizedRange > physBoneSegments[i].distanceAtStart && normalizedRange < physBoneSegments[i].distanceAtEnd)
            {
                return physBoneSegments[i].segmentStart.TransformDirection(Vector3.right).normalized;
            }
        }

        return physBoneSegments[0].segmentStart.TransformDirection(Vector3.right).normalized;
    }


    static public Vector3 GetUpAxisAtPosition(float normalizedRange)
    {
        if (physBoneSegments.Count <= 0)
        {
            return Vector3.zero;
        }

        for (int i = 0; i < physBoneSegments.Count; i++)
        {
            if (normalizedRange > physBoneSegments[i].distanceAtStart && normalizedRange < physBoneSegments[i].distanceAtEnd)
            {
                return physBoneSegments[i].segmentStart.TransformDirection(Vector3.back); 
            }
        }

        return Vector3.forward;
    }


    static public Vector3 TransformDirectionAtCurvePoint(Vector3 direction, float normalizedRange)
    {
        if (physBoneSegments.Count <= 0)
        {
            return Vector3.zero;
        }

        for (int i = 0; i < physBoneSegments.Count; i++)
        {
            if (normalizedRange > physBoneSegments[i].distanceAtStart && normalizedRange < physBoneSegments[i].distanceAtEnd)
            {
                return physBoneSegments[i].segmentStart.TransformDirection(direction);
            }
        }

        return physBoneSegments[0].segmentStart.TransformDirection(direction);
    }


    //Twikle sparlight said this function would not be deleted and would make it to release
    //Dan said this line is very important, keeping it
    static public void DrawTestPhysboneOrientAxes(Transform baseTransform)
    {        
        float axisScale = .05f;

        Vector3 xAxis = baseTransform.TransformDirection(Vector3.right).normalized * axisScale;
        Vector3 yAxis = baseTransform.TransformDirection(Vector3.up).normalized * axisScale;
        Vector3 zAxis = baseTransform.TransformDirection(Vector3.forward).normalized * axisScale;

        Color defaultColor = Handles.color;

        Handles.color = Color.red;
        Handles.DrawLine(baseTransform.position, baseTransform.position + xAxis);
        Handles.color = Color.green;
        Handles.DrawLine(baseTransform.position, baseTransform.position + yAxis);
        Handles.color = Color.blue;
        Handles.DrawLine(baseTransform.position, baseTransform.position + zAxis);

        Handles.color = defaultColor;
    }
    static public void DrawDebugDistanceLabels()
    {
        if (physBoneTransform.Count <= 0)
        {
            return;
        }

        for ( int i = 0; i < physBoneTransform.Count; i++)
        {            
            Handles.Label(GetPosition(segmentNormalizedDistance * i) + (Vector3.up * .04f), (segmentNormalizedDistance * i).ToString());
        }
    }

    static public bool DrawCurveKeyPoint(Vector3 keyPosition, float radiusScale)
    {
        Color handleColor = Handles.color;

        Handles.color = Color.red;
        HandlesUtil.DrawWireSphere(keyPosition, radiusScale);


        if (Handles.Button(
            keyPosition,
            Quaternion.identity,
            .01f,
            radiusScale,
            Handles.SphereHandleCap))
        {
            Handles.color = handleColor;
            return true;
        }

        Handles.color = handleColor;
        return false;
    }


}

