using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.Dynamics;

/// <summary>
/// This class is in charge of analyzing the gameobject we have selected and send 
/// proper data to the required classes for editing, every class that requires
/// selection information should read from here
/// </summary>
public class Nimbat_SelectionData
{
    public delegate void SelectionChanged();
    static public SelectionChanged OnSelectionChanged;
    
    static public bool selectionHasMirrorSuffix;        //regardles if mirror is set correctly or exists, the selected object has _L or _R suffix

    static public NimbatVRCObjectBase selectedVRCNimbatObject;
    static public NimbatMirrorObject nimbatMirrorData;

    static ContactBase selectedContact;
    static VRCPhysBoneBase physBone;
    static GameObject _activeSelectedGameObject;

    static bool isPrefab;

    static public GameObject activeSelectedGameobject
    {
        get
        {
            return _activeSelectedGameObject;
        }
        set
        {
            if (value)
            {
                isPrefab = NimbatFunctions.IsPrefab(value);

                //--first we search for a contact component
                selectedContact = value.GetComponent<ContactBase>();
                physBone = value.GetComponent<VRCPhysBoneBase>();

                if (selectedContact)
                {
                    selectedVRCNimbatObject.contact = selectedContact;  
                }
                else if (physBone)
                {
                    selectedVRCNimbatObject.physBone = physBone;
                }
                else
                {
                    //--if we dont have an object compatible with NimbatVRCObjectBase we clear the data
                    selectedVRCNimbatObject.ClearData();
                }

                if (selectedVRCNimbatObject.mirrorType != MirrorTypes.None)
                {
                    nimbatMirrorData = NimbatCore.vrcMirrorGroups.GetMirrorObjectDataFromGameObject(selectedContact.gameObject);
                    selectionHasMirrorSuffix = true;
                }
                else
                {
                    nimbatMirrorData = null;
                    selectionHasMirrorSuffix = false;
                }

            }
            else
            {
                selectedVRCNimbatObject.ClearData();

                nimbatMirrorData = null;
                selectionHasMirrorSuffix = false;
                isPrefab = false;
            }

            _activeSelectedGameObject = value;
            OnSelectionChanged?.Invoke();
        }
    }



    static public void ForceSelectionChange()
    {
        activeSelectedGameobject = activeSelectedGameobject;
        OnSelectionChanged?.Invoke();
    }

}
