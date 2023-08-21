using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class NimbatTangent
{
    public float tangent = 0;

    public float angleRad
    {
        get
        {
            return Mathf.Atan(tangent);
        }
        set
        {
            tangent = Mathf.Tan(value);
        }
    }
    public float angleDeg
    {
        get
        {
            return angleRad * Mathf.Rad2Deg;
        }
        set
        {
            angleRad = value * Mathf.Deg2Rad;
        }
    }

    public Vector3 tangentVector
    {
        get
        {
            return Quaternion.Euler(angleDeg, 0, 0) * Vector3.right;
        }
    }
    
}

public class Nimbat_PhysboneEditor : NimbatCutieInspectorWindow
{
    static VRCPhysBoneBase activePhysbone;    
    
    static int selectedKeyID;                 
    
    static Keyframe selectedKeyOriginalData;
    

    static Quaternion rotation = Quaternion.identity;

    //--physbone data
    static float physBone_lenght
    {
        get
        {
            return NimbatPhysBoneDrawer.physBone_Lenght;
        }
    }
    static float physBone_radius
    {
        get
        {
            if(activePhysbone)
                return activePhysbone.radius;

            return 0;
        }
    }

    //--tangent editing
    Quaternion tangentRotation;

    static AnimationCurve tempCurve;

    static Vector3 tangentOut_StartPos;
    static Vector3 tangentIn_StartPos;


    static Vector3 tangent_editedHandleRotation;
    static Vector3 outHandleRotationVector;

    static float angle_OutVector;
    static float angle_InVector;

    //--position edit
    static Vector3 keyOriginalPosition;
    static Vector3 keyEditPosition;

    static float startTime;
    static float dotProduct;
    static float editedPositionDistance;
    static float keyOriginalTime;

    float iterations = 15;

    //--main options
    bool enableTool = false;

    #region ======================================== Constructor and destructor

    public Nimbat_PhysboneEditor()
    {
        title = "Physbone";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 150;        

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
    }

    ~Nimbat_PhysboneEditor()
    {
        Nimbat_SelectionData.OnSelectionChanged -= OnSelectionChanged;
    }

    #endregion

    #region =================================== Cutie Inspector Overrides

    public override bool IsWindowValid()
    {
        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.PhysBone)
        {
            return true;
        }
        return false;
    }

    void OnSelectionChanged()
    {
        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.PhysBone)
        {
            activePhysbone = Nimbat_SelectionData.selectedVRCNimbatObject.physBone;
            selectedKeyID = 0;

            NimbatPhysBoneDrawer.UpdateChain(activePhysbone.rootTransform);
            tempCurve = activePhysbone.radiusCurve;

            SelectKeyframe(0);

            NimbatCore.overrideDelKey = true;
        }
        else
        {
            NimbatCore.overrideDelKey = false;
        }
    }

    public override void CutieInspectorContent()
    {
        GUILayout.Label("Physbone controls still in alpha", EditorStyles.miniLabel);

        if (!activePhysbone)
        {
            return;
        }

        if (!activePhysbone.rootTransform)
        {
            GUILayout.Label("no root bone set for this physbone");
            return;
        }

        enableTool = GUILayout.Toggle(enableTool, "enable physbone handles", EditorStyles.miniButton);

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Add curve point"))
        {
            int currentKey = selectedKeyID;

            if(currentKey >= activePhysbone.radiusCurve.keys.Length-1)
            {
                currentKey--;
            }

            float midKeyframePosition = activePhysbone.radiusCurve.keys[currentKey].time + ((activePhysbone.radiusCurve.keys[currentKey + 1].time - activePhysbone.radiusCurve.keys[currentKey].time) * .5f);            
            activePhysbone.radiusCurve.AddKey(midKeyframePosition, activePhysbone.radiusCurve.keys[currentKey].value);

            SelectKeyframe(currentKey+1);

        }

        if (GUILayout.Button("Delete curve point"))
        {
            activePhysbone.radiusCurve.RemoveKey(selectedKeyID);
            SelectKeyframe(0);
        }
    }

    public override void CutieInspectorHandles()
    {        
        //--for some reason, if i dont draw this handle first, nothing else works        //Dan said this line is very important, this is not a meme or a joke     
        Handles.Label(Vector3.zero, string.Empty);

        
        if (!activePhysbone.rootTransform)
        {
            return;
        }

        if (activePhysbone.radiusCurve == null || activePhysbone.radiusCurve.length <= 0)
        {
            SetupCurveDefaultKeyframes();            
            return;
        }

        tempCurve = activePhysbone.radiusCurve;

        NimbatPhysBoneDrawer.DrawDebugDistanceLabels();

        Handles.color = Color.green;
        DrawPhysboneRadiusCurve();

        DrawRadiusKeys();

        DrawEditKeyHandles();

        #region === OLD CODE

        /*

        if (NimbatCore.ctrlDown)
        {
            DrawAddKeyHandles();
            DrawEditRadiusHandle();
        }
        else
        {
            for (int i = 0; i < tempCurve.keys.Length; i++)
        {        
            if (i == selectedKeyID)
            {
                if (NimbatCore.delDown && NimbatCore.keyEventType == EventType.KeyDown)
                {
                    tempCurve.RemoveKey(i);
                    
                    activePhysbone.radiusCurve = tempCurve;

                    SelectKeyframeByID(0);                    
                }

                continue;
            }

            float keyValue = tempCurve.keys[i].value;
            Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(tempCurve.keys[i].time);
            float radiusScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(tempCurve.keys[i].time) * keyValue) * activePhysbone.radius;            

            //--we selected a new keyframe
            if (NimbatPhysBoneDrawer.DrawCurveKeyPoint(keyPosition, radiusScale))
            {
                selectedKeyID = i;
                SelectKeyframeByID(i);
            }

        }
            DrawKeyEditHandles();
        }
        */
        #endregion
    }

    #endregion

    void DrawPhysboneRadiusCurve()
    {
        int curveIterations = (int) iterations;
        float segmentSize = 1 / (float) (iterations-1);
        float segmentPos;

        NimbatPhysBoneCurvePoint startPoint = new NimbatPhysBoneCurvePoint();
        NimbatPhysBoneCurvePoint endPoint = new NimbatPhysBoneCurvePoint();

        for (int i = 0; i<= curveIterations; i++)
        {
            segmentPos = segmentSize * i;
            
            if( i == 0)
            {
                startPoint = NimbatPhysBoneDrawer.GetPointCurveAtPosition(segmentPos, activePhysbone);                
                continue;
            }

            endPoint = NimbatPhysBoneDrawer.GetPointCurveAtPosition(segmentPos, activePhysbone);
                                   
            Handles.DrawLine(startPoint.positionScaled, endPoint.positionScaled);

            Handles.DrawLine(startPoint.position, endPoint.position);

            Handles.DrawLine(startPoint.positionScaledInverted, endPoint.positionScaledInverted);
           
            startPoint = endPoint;            
        }
    }

    /// <summary>
    /// Called to mark a new keyframe as selected, int id is the id of the radius curve we are currently editing
    /// </summary>    
    void SelectKeyframe(int id)
    {
        if(activePhysbone.radiusCurve == null || activePhysbone.radiusCurve.length <= 0)
        {
            selectedKeyID = 0;
            return;
        }

        selectedKeyID = Mathf.Clamp(id,0,activePhysbone.radiusCurve.length -1);
        selectedKeyOriginalData = activePhysbone.radiusCurve.keys[selectedKeyID];

        keyOriginalPosition = NimbatPhysBoneDrawer.GetPosition(selectedKeyOriginalData.time);
        keyEditPosition = keyOriginalPosition;
        keyOriginalTime = selectedKeyOriginalData.time;

        tangentRotation = Quaternion.identity;

        

        /*
        //--reset quaternion rotation data
        rotation = Quaternion.identity;        
        outHandleRotationVector = NimbatPhysBoneDrawer.GetRotationAxisAtPosition(selectedKeyOriginalData.time);

        //--get the vector start positions that represents the tangent angle
        tangentOut_StartPos = TangentVector(selectedKeyOriginalData.outTangent, selectedKeyOriginalData.time,Vector3.up);        
        tangentIn_StartPos = TangentVector(selectedKeyOriginalData.inTangent, selectedKeyOriginalData.time, Vector3.down);


        startTime = selectedKeyOriginalData.time;*/
    }

    void DrawRadiusKeys()
    {
        Keyframe[] keys = activePhysbone.radiusCurve.keys;

        NimbatPhysBoneCurvePoint keyframePoint;

        bool ctrlDown = NimbatCore.ctrlDown;
        bool shiftDown = NimbatCore.shiftDown;
        Vector3 midPosition;
        float midKeyframePosition;

        for (int i = 0; i< keys.Length; i++)
        {
            keyframePoint = NimbatPhysBoneDrawer.GetPointCurveAtPosition(keys[i].time, activePhysbone);

            HandlesUtil.DrawWireSphere(keyframePoint.positionScaled, .005f);
            Handles.DrawWireArc(keyframePoint.position, keyframePoint.forwardDirection, keyframePoint.directionScaled, 360, keyframePoint.radius);


            if (!ctrlDown)
                continue;

            if(NimbatHandles.DrawSphereButton(keyframePoint.position,.01f, Color.green))
            {
                SelectKeyframe(i);
            }

            if (!shiftDown || i >= keys.Length -1)
                continue;

            midKeyframePosition = keys[i].time + ((keys[i + 1].time - keys[i].time) * .5f);
            midPosition = NimbatPhysBoneDrawer.GetPosition(midKeyframePosition);

            if (NimbatHandles.DrawSphereButton(midPosition, .01f, Color.green))
            {
                activePhysbone.radiusCurve.AddKey(midKeyframePosition, keys[i].value);
            }            
        }
    }

    void DrawEditKeyHandles()
    {
        Keyframe currentKey = activePhysbone.radiusCurve.keys[selectedKeyID];
        
        NimbatPhysBoneCurvePoint keyframePoint = NimbatPhysBoneDrawer.GetPointCurveAtPosition(currentKey.time, activePhysbone);

        float keyValue = currentKey.value;
        float absoluteScale = NimbatPhysBoneDrawer.GetAbsoluteScale(currentKey.time);
        Vector3 upDirection = keyframePoint.direction.normalized;
        Vector3 rotationHandle = NimbatPhysBoneDrawer.GetRotationAxisAtPosition(currentKey.time);


        Handles.DrawLine(keyframePoint.position, keyframePoint.positionScaled);
        
        Vector3 newRadiusPosition = Handles.Slider(keyframePoint.position  + (upDirection * ((keyValue * activePhysbone.radius) * absoluteScale)), keyframePoint.direction);

        Handles.color = Color.red;

        keyEditPosition = Handles.Slider(keyEditPosition, keyframePoint.forwardDirection);
        HandlesUtil.DrawWireSphere(keyEditPosition, .005f);

        Handles.color = Color.blue;

        tangentRotation = Handles.Disc(tangentRotation, keyframePoint.positionScaled, rotationHandle, .05f, false, 1);


        //--get the vector start positions that represents the tangent angle
        tangentIn_StartPos = TangentVector(selectedKeyOriginalData.inTangent, selectedKeyOriginalData.time, Vector3.down);
        tangentOut_StartPos = TangentVector(selectedKeyOriginalData.outTangent, selectedKeyOriginalData.time, Vector3.up);

        Handles.color = Color.white;

        
        HandlesUtil.DrawWireSphere(keyframePoint.positionScaled + (tangentRotation * (tangentOut_StartPos * .1f)), .002f);
        HandlesUtil.DrawWireSphere(keyframePoint.positionScaled + (tangentRotation * (tangentIn_StartPos * .1f)), .002f);

        angle_InVector = Vector3.Angle(tangentRotation * tangentIn_StartPos, NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.down, currentKey.time));
        angle_OutVector = Vector3.Angle(tangentRotation * tangentOut_StartPos, NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.up, currentKey.time));


        float angleFromTop = Vector3.Angle(tangentRotation * tangentIn_StartPos, NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.back, currentKey.time));
        

        Handles.DrawLine(keyframePoint.positionScaled, keyframePoint.positionScaled + (tangentRotation * (tangentOut_StartPos * .1f)));
        Handles.DrawLine(keyframePoint.positionScaled, keyframePoint.positionScaled + (tangentRotation * (tangentIn_StartPos * .1f)));


        /*
        
        if (selectedKeyOriginalData.inTangent < 0)
        {
            angle_InVector *= -1;
        }
        if (selectedKeyOriginalData.outTangent < 0)
        {
            angle_OutVector *= -1;
        }
        */

        if (angleFromTop > 90)
        {
            angle_InVector *= -1;
            angle_OutVector *= -1;
        }



        //--calculates the distance for the new time 
        float newKeyTimeOffset = Vector3.Distance(keyOriginalPosition, keyEditPosition) / physBone_lenght;
        dotProduct = Vector3.Dot(keyOriginalPosition - keyEditPosition, keyframePoint.forwardDirection);
        if (dotProduct > 0)
        {
            newKeyTimeOffset *= -1;
        }

        //--calculates the distance for the new radius
        float newValueOffset = Vector3.Distance(keyframePoint.position, newRadiusPosition);

        //--transfer data to the keyframe
        Keyframe newKey = currentKey; 

        newKey.time = keyOriginalTime + newKeyTimeOffset;
        newKey.value = (newValueOffset / activePhysbone.radius) / absoluteScale;
        
        //--new tangent value
        newKey.inTangent = AngleToTangent(angle_InVector);
        newKey.inWeight = currentKey.inWeight;
        newKey.outTangent = AngleToTangent(angle_OutVector);
        newKey.outWeight = currentKey.outWeight;

        selectedKeyID = activePhysbone.radiusCurve.MoveKey(selectedKeyID, newKey);        


    }


    #region =========== OLD CODE
    void DrawEditRadiusHandle()
    {
        float absoluteScale = NimbatPhysBoneDrawer.GetAbsoluteScale(0);
        
        float newRadius = Handles.RadiusHandle(Quaternion.identity, NimbatPhysBoneDrawer.GetPosition(0), (activePhysbone.radius * absoluteScale));

        activePhysbone.radius = (newRadius / absoluteScale);
    }


    void DrawKeyEditHandles()
    {        
        //--get the selected key id
        int id = selectedKeyID;

        selectedKeyID = Mathf.Clamp(selectedKeyID, 0, activePhysbone.radiusCurve.keys.Length - 1);

        //--reference to the animation curve
        AnimationCurve tempCurve = activePhysbone.radiusCurve;

        //--reference to the selected keyframe we are going to edit
        selectedKeyOriginalData = tempCurve.keys[ Mathf.Clamp( id, 0, tempCurve.keys.Length-1)];
        //--shortcuts for keyframe data
        float keyValue = selectedKeyOriginalData.value;
        float keyTime = selectedKeyOriginalData.time;

        Vector3 keyPosition = NimbatPhysBoneDrawer.GetPosition(keyTime);
        float absoluteScale = (NimbatPhysBoneDrawer.GetAbsoluteScale(keyTime));
        float scaledValue = keyValue * activePhysbone.radius;

        Handles.color = Color.green;

        //--handles for modifying the radius of the phys bone
        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            keyPosition,
            scaledValue * absoluteScale);

        float scaledRadius = ((newRadius / activePhysbone.radius) / absoluteScale);

        //--this part takes care of the tangets
        Vector3 tangentAxis = NimbatPhysBoneDrawer.GetUpAxisAtPosition(selectedKeyID);


        Handles.color = Color.blue;


        Quaternion newRotation = Handles.Disc(rotation, keyPosition, outHandleRotationVector, .5f, false, 1);                
        rotation = newRotation;        

        tangent_editedHandleRotation = rotation * tangentOut_StartPos;

        Handles.DrawLine(keyPosition, keyPosition + (rotation * (tangentIn_StartPos *.15f)));
        Handles.DrawLine(keyPosition, keyPosition + (rotation * (tangentOut_StartPos * .15f)));

        angle_InVector = Vector3.Angle(rotation * tangentIn_StartPos, NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.down, keyTime));
        angle_OutVector = Vector3.Angle(rotation * tangentOut_StartPos, NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.up, keyTime));

        if(selectedKeyOriginalData.inTangent < 0)
        {
            angle_InVector *= -1;
        }
        if (selectedKeyOriginalData.outTangent < 0)
        {
            angle_OutVector *= -1;
        }

        //--this part takes care of the key position

        Vector3 slideForward = NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.up, keyTime);
               
        keyEditPosition = Handles.Slider(keyEditPosition, slideForward, .05f, Handles.ArrowHandleCap, 0);
        editedPositionDistance = (Vector3.Distance(keyOriginalPosition, keyEditPosition) / physBone_lenght);

        dotProduct = Vector3.Dot(keyOriginalPosition - keyEditPosition, slideForward);

        if(dotProduct > 0)
        {
            editedPositionDistance *= -1;
        }                    

        #region ======================= keyframe data      

        Keyframe newKey = new Keyframe();

        //--new radius
        newKey.value = scaledRadius;

        //--new time
        newKey.time = Mathf.Clamp( startTime + editedPositionDistance,0,1);
        
        //--new tangent value
        newKey.inTangent = AngleToTangent(angle_InVector);
        newKey.inWeight = tempCurve.keys[id].inWeight;        
        newKey.outTangent = AngleToTangent(angle_OutVector);
        newKey.outWeight = tempCurve.keys[id].outWeight;        

        selectedKeyID = activePhysbone.radiusCurve.MoveKey(id, newKey);

        #endregion
    }

    void DrawAddKeyHandles()
    {        
        Color buttonColor = new Color(0, 1, 0, .5f);
        for (int i = 0; i< tempCurve.keys.Length-1; i++)
        {
            float midPoint = tempCurve.keys[i].time + ((tempCurve.keys[i+1].time - tempCurve.keys[i].time)*.5f);
                        
            if (NimbatHandles.DrawSphereButtonInCurve(midPoint, .05f, buttonColor))
            {
                tempCurve.AddKey(midPoint, tempCurve.Evaluate(midPoint));
            }
        }
    }

    void DrawDeleteKeyHandles()
    {
        Color buttonColor = new Color(1, 0, 0, .5f);
        for (int i = 0; i < tempCurve.keys.Length; i++)
        {
            float keyPosition = tempCurve.keys[i].time;

            if (NimbatHandles.DrawSphereButtonInCurve(keyPosition, .05f, buttonColor))
            {
                tempCurve.RemoveKey(i);
            }
        }
    }


    /// <summary>
    /// used to create the tangent vector following the physbone curve, 
    /// </summary>
    /// <param name="tangent">tangent from the key </param>
    /// <param name="curvePosition">normalized position of vrc curve</param>
    /// <param name="baseDirection">vector to get the angle from</param>
    /// <param name="scale">used to display tangents coorectly according to physbone chain scale</param>
    /// <returns></returns>
    Vector3 TangentVector(float tangent, float curvePosition, Vector3 baseDirection)
    {
        Vector3 tangentVector = NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(baseDirection, curvePosition);
        Vector3 rotationAxis = NimbatPhysBoneDrawer.GetRotationAxisAtPosition(curvePosition).normalized;

        float angle = (Mathf.Atan(tangent) * Mathf.Rad2Deg);
         
        tangentVector = NimbatFunctions.RotateVectorOnAxis(tangentVector, rotationAxis, angle);

        return tangentVector.normalized;
    }

    float AngleToTangent(float angle)
    {
        return Mathf.Tan(angle * Mathf.Deg2Rad);
    }

    /// <summary>
    /// When a physbone radius curve is empty we need to create a start and end keyframe
    /// to be able to start editing it
    /// </summary>
    void SetupCurveDefaultKeyframes()
    {       
        activePhysbone.radiusCurve.AddKey(0, 1);
        activePhysbone.radiusCurve.AddKey(1, 1);

        if(physBone_radius <= 0)
        {
            activePhysbone.radius = .0001f;
        }

        tempCurve = activePhysbone.radiusCurve;

        SelectKeyframe(0);
    }

    #endregion
}
