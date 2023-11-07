using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Variables/StringValue")]
[System.Serializable]
public class StringValue : VariablesValue
{

    public string initialValue;


    public string RuntimeValue;
    
    public StringValue Clone()
    {
        StringValue value = new StringValue();
        value.initialValue = initialValue;
        value.RuntimeValue = RuntimeValue;
        return value;
    }
    
    public override void LoadValue(BinaryReader data)
    {
        RuntimeValue = data.ReadString();
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
