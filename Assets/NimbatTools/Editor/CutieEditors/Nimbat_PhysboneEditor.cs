using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using VRC.Dynamics;

public class Nimbat_PhysboneEditor : NimbatCutieInspectorWindow
{
    static VRCPhysBoneBase activePhysbone;    
    
    static int selectedKeyID;                 
    static Keyframe selectedKey;
    static Keyframe editedKey;

    static float outTanValue;
    static float outHandleAngle;

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
    static AnimationCurve tempCurve;

    static Vector3 tangentOut_StartPos;
    static Vector3 tangentIn_StartPos;


    static Vector3 tangent_editedHandleRotation;
    static Vector3 outHandleRotationVector;

    static float angle_OutVector;
    static float angle_InVector;

    //--position edit
    static Vector3 startPosition;
    static Vector3 editedPosition;

    static float startTime;
    static float dotProduct;
    static float editedPositionDistance;

    #region ======================================== Constructor and destructor

    public Nimbat_PhysboneEditor()
    {
        title = "Physbone";
        drawModes = CutieInspectorDrawModes.DropUp;
        width = NimbatData.settingsWindowsWidth;
        height = 220;        

        Nimbat_SelectionData.OnSelectionChanged += OnSelectionChanged;
    }

    ~Nimbat_PhysboneEditor()
    {
        Nimbat_SelectionData.OnSelectionChanged -= OnSelectionChanged;
    }

    #endregion

    void OnSelectionChanged()
    {
        if (Nimbat_SelectionData.selectedVRCNimbatObject.vrcObjectType == VRCObjectType.PhysBone)
        {
            activePhysbone = Nimbat_SelectionData.selectedVRCNimbatObject.physBone;
            selectedKeyID = 0;

            NimbatPhysBoneDrawer.UpdateChain(activePhysbone.rootTransform);
            tempCurve = activePhysbone.radiusCurve;
            SelectKeyframeByID(0);

            NimbatCore.overrideDelKey = true;
        }
        else
        {
            NimbatCore.overrideDelKey = false;
        }
    }

    public override void CutieInspectorContent()
    {

        if (!activePhysbone.rootTransform)
        {

            GUILayout.Label("no root bone set for this physbone");
            GUILayout.Label("------ ");
            return;
        }

        /*
        GUILayout.Label("Physbone lenght " + physBone_lenght, EditorStyles.miniLabel);
        GUILayout.Label("Physbone radius " + physBone_radius, EditorStyles.miniLabel);

        GUILayout.Label("------ ");

        GUILayout.Label("Tangent in " + currentKey.inTangent, EditorStyles.miniLabel);
        GUILayout.Label("Tangent out " + currentKey.outTangent, EditorStyles.miniLabel);
        
        GUILayout.Label("------ ");

        GUILayout.Label("Angle tangent in " + TangentToAngle(currentKey.inTangent) , EditorStyles.miniLabel);
        GUILayout.Label("Angle tangent out " + TangentToAngle(currentKey.outTangent), EditorStyles.miniLabel);

        GUILayout.Label("------ ");

        GUILayout.Label("Angle from in vector "  + angle_InVector.ToString(), EditorStyles.miniLabel);
        GUILayout.Label("Angle from out vector " + angle_OutVector.ToString(), EditorStyles.miniLabel);

        GUILayout.Label("------ ");

        GUILayout.Label("Tangent from in vector " + AngleToTangent(angle_InVector).ToString(), EditorStyles.miniLabel);
        GUILayout.Label("Tangent from out vector " + AngleToTangent(angle_OutVector).ToString(), EditorStyles.miniLabel);
        */

        GUILayout.Label("phys segments count " + NimbatPhysBoneDrawer.physBoneSegments.Count.ToString());

        GUILayout.Label("------ ");
        GUILayout.Label("Distance " + Vector3.Distance(startPosition, editedPosition));

        GUILayout.Label("key time " + selectedKey.time);
        GUILayout.Label("key value " + selectedKey.value);

        GUILayout.Label("new value distance " + editedPositionDistance.ToString());        
    }

    public override void CutieInspectorHandles()
    {
        //--for some reason, if i dont draw this handle first, nothing else works
        //Dan said this line is very important, this is not a meme or a joke     
        Handles.Label(Vector3.zero, "");

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
    }

    /// <summary>
    /// Called to mark a new keyframe as selected, int id is the id of the radius curve we are currently editing
    /// </summary>    
    void SelectKeyframeByID(int id)
    {


        if(activePhysbone.radiusCurve == null || activePhysbone.radiusCurve.length <= 0)
        {
            selectedKeyID = 0;
            return;
        }

        selectedKeyID = Mathf.Clamp(id,0,activePhysbone.radiusCurve.length -1);

        selectedKey = activePhysbone.radiusCurve.keys[selectedKeyID];

        //--reset quaternion rotation data
        rotation = Quaternion.identity;        

        outHandleRotationVector = NimbatPhysBoneDrawer.GetRotationAxisAtPosition(selectedKey.time);

        //--get the vector start positions that represents the tangent angle
        tangentOut_StartPos = TangentVector(selectedKey.outTangent, selectedKey.time,Vector3.up);        
        tangentIn_StartPos = TangentVector(selectedKey.inTangent, selectedKey.time, Vector3.down);

        startPosition = NimbatPhysBoneDrawer.GetPosition(selectedKey.time);
        editedPosition = startPosition;

        startTime = selectedKey.time;
    }

    void DrawEditRadiusHandle()
    {
        float absoluteScale = NimbatPhysBoneDrawer.GetAbsoluteScale(0);
        float newRadius = activePhysbone.radius;
        float offset = .05f;

        newRadius = Handles.RadiusHandle(Quaternion.identity, NimbatPhysBoneDrawer.GetPosition(0), (activePhysbone.radius * absoluteScale));

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
        selectedKey = tempCurve.keys[ Mathf.Clamp( id, 0, tempCurve.keys.Length-1)];
        //--shortcuts for keyframe data
        float keyValue = selectedKey.value;
        float keyTime = selectedKey.time;

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

        if(selectedKey.inTangent < 0)
        {
            angle_InVector *= -1;
        }
        if (selectedKey.outTangent < 0)
        {
            angle_OutVector *= -1;
        }

        //--this part takes care of the key position

        Vector3 slideForward = NimbatPhysBoneDrawer.TransformDirectionAtCurvePoint(Vector3.up, keyTime);
               
        editedPosition = Handles.Slider(editedPosition, slideForward, .05f, Handles.ArrowHandleCap, 0);
        editedPositionDistance = (Vector3.Distance(startPosition, editedPosition) / physBone_lenght);

        dotProduct = Vector3.Dot(startPosition - editedPosition, slideForward);

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
                        
            if (NimbatHandles.DrawSphereButton(midPoint, .05f, buttonColor))
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

            if (NimbatHandles.DrawSphereButton(keyPosition, .05f, buttonColor))
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
        activePhysbone.radius = .0001f;

        tempCurve = activePhysbone.radiusCurve;

        SelectKeyframeByID(0);
    }

}
