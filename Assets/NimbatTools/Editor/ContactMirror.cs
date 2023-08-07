using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class ContactMirror : MonoBehaviour
{
    public VRC.Dynamics.ContactBase baseContact;
    public VRC.Dynamics.ContactBase targetContact;


    private void OnDrawGizmos()
    {
        if (baseContact)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(baseContact.transform.position + baseContact.position, baseContact.radius);
        }
        if (targetContact)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetContact.transform.position + targetContact.position, targetContact.radius);
        }
    }


    private void Update()
    {
        if (!baseContact)
            return;

        if (targetContact)
        {
            targetContact.radius = baseContact.radius;
        }
    }


}
