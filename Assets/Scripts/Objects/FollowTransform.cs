using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public bool offsetSetOnStart = false;
    public bool followPosition = true;
    public bool followRotation = true;
    public bool followScale = true;
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;
    public bool followXRotation = true;
    public bool followYRotation = true;
    public bool followZRotation = true;
    public bool followXScale = true;
    public bool followYScale = true;
    public bool followZScale = true;
    // Start is called before the first frame update
    void Awake()
    {
        if (offsetSetOnStart)
        {
            offset = transform.position - target.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        Vector3 scale = transform.localScale;
        if (followPosition)
        {
            if (followX)
                pos.x = target.position.x + offset.x;
            if (followY)
                pos.y = target.position.y + offset.y;
            if (followZ)
                pos.z = target.position.z + offset.z;
        }
        if (followRotation)
        {
            if (followXRotation)
                rot.x = target.eulerAngles.x;
            if (followYRotation)
                rot.y = target.eulerAngles.y;
            if (followZRotation)
                rot.z = target.eulerAngles.z;
        }
        if (followScale)
        {
            if (followXScale)
                scale.x = target.localScale.x;
            if (followYScale)
                scale.y = target.localScale.y;
            if (followZScale)
                scale.z = target.localScale.z;
        }
        transform.position = pos;
        transform.eulerAngles = rot;
        transform.localScale = scale;
    }
}
