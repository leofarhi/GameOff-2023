using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCollider : MonoBehaviour
{
    private CharacterController capsuleCollider;
    private Vector3 lastPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CharacterController>();
        lastPosition = transform.position;
    }
    
    public void ForceTeleport(Vector3 position)
    {
        lastPosition = position;
        transform.position = position;
    }
    
    // Update is called once per frame
    private void FixedUpdate()
    {
        if (IsOnGround())
        {
            //Debug.Log("On Ground");
            lastPosition = transform.position;
        }
        else
        {
            //Debug.Log("Not On Ground");
            Vector3 position = transform.position;
            //transform.position = new Vector3(lastPosition.x, transform.position.y, lastPosition.z);
            transform.position = new Vector3(lastPosition.x, position.y, position.z);
            if (IsOnGround())
            {
                lastPosition = transform.position;
                return;
            }
            transform.position = new Vector3(position.x, position.y, lastPosition.z);
            if (IsOnGround())
            {
                lastPosition = transform.position;
                return;
            }
            transform.position = new Vector3(lastPosition.x, position.y, lastPosition.z);
            if (IsOnGround())
            {
                lastPosition = transform.position;
                return;
            }
            transform.position = lastPosition;
        }
    }
    

    public bool IsOnGround()
    {
        //send raycast from current position to bottom of capsule collider
        //send 4 raycasts from each corner of the capsule collider to the bottom of the capsule collider
        //Distance can be infinite
        RaycastHit hit;
        float radius = capsuleCollider.radius - 0.1f;
        Vector3[] StartPositions =
        {
            capsuleCollider.center + new Vector3(radius, 0, radius),
            capsuleCollider.center + new Vector3(-radius, 0, radius),
            capsuleCollider.center + new Vector3(radius, 0, -radius),
            capsuleCollider.center + new Vector3(-radius, 0, -radius),
            capsuleCollider.center + new Vector3(0, 0, 0)
        };
        //Is variable if all raycasts hit something
        bool isOnGround = true;
        LayerMask layerMask = LayerMask.GetMask("GroundCollider");
        foreach (var startPosition in StartPositions)
        {
            Debug.DrawRay(transform.TransformPoint(startPosition), Vector3.down * 100, Color.red);
            //Cast Only GroundCollider Layer
            if (!Physics.Raycast(transform.TransformPoint(startPosition), Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                //If any raycast doesn't hit something, return false
                isOnGround = false;
                break;
            }
        }
        return isOnGround;
    }
}
