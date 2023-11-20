using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraArea : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 shift;
    public bool smoothRotation = true;
    public int priority = 0;
    public float size = 2f;
    public float shiftCameraRotationY = 0f;
    public bool useOnlyShiftRotation = false;
    void OnDrawGizmos()
    {
        Color color = Color.green;
        color.a = 0.5f;
        Gizmos.color = color;
        //dessine un cube à la position du trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boxCollider.center = Vector3.zero;
        boxCollider.size = Vector3.one;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();
        if (player != null)
        {
            player.CameraMain._cameraAreas.Add(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();
        if (player != null && player.CameraMain._cameraAreas.Contains(this))
        {
            player.CameraMain._cameraAreas.Remove(this);
        }
    }
}
