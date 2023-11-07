using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Isometric Camera
public class ThirdPersonCamera : MonoBehaviour
{
    public ThirdPersonController player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    
    
    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 desiredPosition = player.transform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
        Quaternion desiredRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
    }
}
