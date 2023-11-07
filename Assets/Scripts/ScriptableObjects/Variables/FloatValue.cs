using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Variables/FloatValue")]
[System.Serializable]
public class FloatValue : VariablesValue
{

    public float initialValue;


    public float RuntimeValue;
    
    public FloatValue Clone()
    {
        FloatValue value = new FloatValue();
        value.initialValue = initialValue;
        value.RuntimeValue = RuntimeValue;
        return value;
    }
    
    public override void LoadValue(BinaryReader data)
    {
        RuntimeValue = data.ReadSingle();
    }

    public override void SaveValue(BinaryWriter writer)
    {
        writer.Write(RuntimeValue);
    }
    
    public override void Reset()
    {
        RuntimeValue = initialValue;
    }

}
