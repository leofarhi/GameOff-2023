using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator2D : MonoBehaviour
{
    [Serializable]
    public struct RotationConstraint
    {
        public bool x;
        public bool y;
        public bool z;
    }
    public Sheet2dValue spriteSheet;
    public Material material;
    public Vector3 rotationOffset;
    [SerializeField]
    public RotationConstraint fixedAxis;
    
    private Sheet2dValue.Animation currentAnimation;
    private int currentFrame;
    private int currentDirection;
    // Start is called before the first frame update
    void Start()
    {
        spriteSheet.InitTextures();
        currentAnimation = spriteSheet.animations[0];
        currentFrame = 0;
        currentDirection = 0;
    }
    
    void LookCamera()
    {
        Vector3 lookPos = Camera.main.transform.position - transform.position;
        if (fixedAxis.x)
        {
            lookPos.x = 0;
        }
        if (fixedAxis.y)
        {
            lookPos.y = 0;
        }
        if (fixedAxis.z)
        {
            lookPos.z = 0;
        }
        Quaternion rotation = Quaternion.LookRotation(lookPos, Vector3.up);
        transform.rotation = rotation * Quaternion.Euler(rotationOffset);
    }
    
    public void SetTexture(Texture2D texture)
    {
        material.mainTexture = texture;
    }

    // Update is called once per frame
    void Update()
    {
        LookCamera();
        SetTexture(currentAnimation.frames[currentFrame].textures[currentDirection]);
    }
}
