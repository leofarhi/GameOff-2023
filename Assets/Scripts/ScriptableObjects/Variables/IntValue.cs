using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Variables/IntValue")]
[System.Serializable]
public class IntValue : VariablesValue
{

    public int initialValue;


    public int RuntimeValue;

    public IntValue Clone()
    {
        IntValue value = new IntValue();
        value.initialValue = initialValue;
        value.RuntimeValue = RuntimeValue;
        return value;
    }
    
    public override void LoadValue(BinaryReader data)
    {
        RuntimeValue = data.ReadInt32();
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
