using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Variables/BoolValue")]
[System.Serializable]
public class BoolValue : VariablesValue
{
    public bool initialValue;


    public bool RuntimeValue;

    
    public BoolValue Clone()
    {
        BoolValue value = new BoolValue();
        value.initialValue = initialValue;
        value.RuntimeValue = RuntimeValue;
        return value;
    }

    public override void LoadValue(BinaryReader data)
    {
        RuntimeValue = data.ReadBoolean();
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
