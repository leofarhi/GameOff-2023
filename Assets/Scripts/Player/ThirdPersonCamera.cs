using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Isometric Camera
public class ThirdPersonCamera : MonoBehaviour
{
    public ThirdPersonController player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public Vector3 shift;
    public bool smoothRotation = false;
    public float size = 2f;

    [HideInInspector]
    public List<CameraArea> _cameraAreas = new List<CameraArea>();

    private void Awake()
    {
        _cameraAreas = new List<CameraArea>();
    }

    void UpdateCameraArea()
    {
        //removes all null values from the list
        _cameraAreas.RemoveAll(item => item == null);
        //sorts the list by priority
        _cameraAreas.Sort((x, y) => x.priority.CompareTo(y.priority));
        //first element of the list is the one with the highest priority
        CameraArea area = _cameraAreas.Count > 0 ? _cameraAreas[0] : null;
        if (area != null)
        {
            offset = area.offset;
            shift = area.shift;
            smoothRotation = area.smoothRotation;
            size = area.size;
        }
    }


    void Update()
    {
        if (player == null)
        {
            return;
        }
        UpdateCameraArea();
        Vector3 desiredPosition = player.transform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position- shift, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        if (smoothRotation)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(player.transform.position - (transform.position- shift));
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
        }
        else
        {
            transform.LookAt(player.transform);
        }
        transform.position += shift;
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, smoothSpeed);
    }
}
