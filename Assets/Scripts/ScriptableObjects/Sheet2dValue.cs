using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Sheet2dValue")]
[System.Serializable]
public class Sheet2dValue : SystemValue
{
    [Serializable]
    public class Frame
    {
        public Sprite right;
        public Sprite left;
        public Sprite up;
        public Sprite down;
        [HideInInspector]
        public Texture2D[] textures;
    }
    [Serializable]
    public struct Animation
    {
        public string name;
        public Frame[] frames;
        public float frameTime;
    }
    public Animation[] animations;
    
    public void InitTextures()
    {
        foreach (Animation animation in animations)
        {
            foreach (Frame frame in animation.frames)
            {
                frame.textures = new Texture2D[4];
                //convert sprite to texture
                frame.textures[0] = SpriteToTexture2D(frame.right);
                frame.textures[1] = SpriteToTexture2D(frame.left);
                frame.textures[2] = SpriteToTexture2D(frame.up);
                frame.textures[3] = SpriteToTexture2D(frame.down);
            }
        }
    }
    
    public static Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

            Color[] newColors = sprite.texture.GetPixels((int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height);
            newText.SetPixels(newColors);
            newText.Apply();
            return newText;
        }
        else
        {
            return sprite.texture;
        }
    }
    
    public Animation GetAnimation(string name)
    {
        foreach (Animation animation in animations)
        {
            if (animation.name == name)
            {
                return animation;
            }
        }
        return animations[0];
    }
}
