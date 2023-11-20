using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/Variables/ListValue")]
[System.Serializable]
public class ListValue : VariablesValue
{
    [System.Serializable]
    public enum Type
    {
        Int,
        Float,
        Bool,
        String,
    }
    
    [System.Serializable]
    public class ListElement
    {
        public Type type;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public string stringValue;

        public ListElement(Type type, object value)
        {
            this.type = type;
            switch (type)
            {
                case Type.Int:
                    intValue = (int)value;
                    break;
                case Type.Float:
                    floatValue = (float)value;
                    break;
                case Type.Bool:
                    boolValue = (bool)value;
                    break;
                case Type.String:
                    stringValue = (string)value;
                    break;
            }
        }
        
        //overwrites equality operator
        public static bool operator ==(ListElement a, ListElement b)
        {
            if (a.type != b.type)
            {
                return false;
            }
            switch (a.type)
            {
                case Type.Int:
                    return a.intValue == b.intValue;
                case Type.Float:
                    return a.floatValue == b.floatValue;
                case Type.Bool:
                    return a.boolValue == b.boolValue;
                case Type.String:
                    return a.stringValue == b.stringValue;
            }
            return false;
        }
        
        //overwrites inequality operator
        public static bool operator !=(ListElement a, ListElement b)
        {
            return !(a == b);
        }
        
        public Tuple<Type,object> Get()
        {
            switch (type)
            {
                case Type.Int:
                    return new Tuple<Type, object>(type, intValue);
                case Type.Float:
                    return new Tuple<Type, object>(type, floatValue);
                case Type.Bool:
                    return new Tuple<Type, object>(type, boolValue);
                case Type.String:
                    return new Tuple<Type, object>(type, stringValue);
            }
            return null;
        }
        
        public void Set(object value)
        {
            switch (type)
            {
                case Type.Int:
                    intValue = (int)value;
                    break;
                case Type.Float:
                    floatValue = (float)value;
                    break;
                case Type.Bool:
                    boolValue = (bool)value;
                    break;
                case Type.String:
                    stringValue = (string)value;
                    break;
            }
        }

        public ListElement Clone()
        {
            Tuple<Type, object> value = Get();
            ListElement element = new ListElement(value.Item1, value.Item2);
            return element;
        }
        
        public static ListElement Load(BinaryReader data)
        {
            Type type = (Type)data.ReadInt32();
            object value = null;
            switch (type)
            {
                case Type.Int:
                    value = data.ReadInt32();
                    break;
                case Type.Float:
                    value = data.ReadSingle();
                    break;
                case Type.Bool:
                    value = data.ReadBoolean();
                    break;
                case Type.String:
                    value = data.ReadString();
                    break;
            }
            return new ListElement(type, value);
        }
        
        public void Save(BinaryWriter writer)
        {
            writer.Write((int)type);
            switch (type)
            {
                case Type.Int:
                    writer.Write(intValue);
                    break;
                case Type.Float:
                    writer.Write(floatValue);
                    break;
                case Type.Bool:
                    writer.Write(boolValue);
                    break;
                case Type.String:
                    writer.Write(stringValue);
                    break;
            }
        }
    }
    
    public List<ListElement> initialValue = new List<ListElement>();
    public List<ListElement> RuntimeValue = new List<ListElement>();
    
    public void AddElement(Type type, object value)
    {
        ListElement element = new ListElement(type, value);
        RuntimeValue.Add(element);
    }
    
    public void RemoveElement(int index)
    {
        RuntimeValue.RemoveAt(index);
    }
    
    public void ClearList()
    {
        RuntimeValue.Clear();
    }
    
    public bool ContainsElement(Type type, object value)
    {
        ListElement temp = new ListElement(type, value);
        foreach (ListElement element in RuntimeValue)
        {
            if (element == temp)
            {
                return true;
            }
        }
        return false;
    }
    
    public int IndexOfElement(Type type, object value)
    {
        ListElement temp = new ListElement(type, value);
        for (int i = 0; i < RuntimeValue.Count; i++)
        {
            if (RuntimeValue[i] == temp)
            {
                return i;
            }
        }
        return -1;
    }
    
    public void RemoveElement(Type type, object value)
    {
        int index = IndexOfElement(type, value);
        if (index != -1)
        {
            RuntimeValue.RemoveAt(index);
        }
    }
    
    public void SetElement(int index, Type type, object value)
    {
        ListElement element = new ListElement(type, value);
        RuntimeValue[index] = element;
    }


    public ListValue Clone()
    {
        ListValue value = new ListValue();
        value.initialValue = new List<ListElement>();
        foreach (ListElement element in initialValue)
        {
            value.initialValue.Add(element.Clone());
        }
        value.RuntimeValue = new List<ListElement>();
        foreach (ListElement element in RuntimeValue)
        {
            value.RuntimeValue.Add(element.Clone());
        }
        return value;
    }

    public override void LoadValue(BinaryReader data)
    {
        int count = data.ReadInt32();
        RuntimeValue = new List<ListElement>();
        for (int i = 0; i < count; i++)
        {
            ListElement element = ListElement.Load(data);
            RuntimeValue.Add(element);
        }
    }

    public override void SaveValue(BinaryWriter writer)
    {
        writer.Write(RuntimeValue.Count);
        foreach (ListElement element in RuntimeValue)
        {
            element.Save(writer);
        }
    }

    public override void Reset()
    {
        RuntimeValue = new List<ListElement>();
        foreach (ListElement element in initialValue)
        {
            RuntimeValue.Add(element.Clone());
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(ListValue))]
public class ListValueEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        ListValue listValue = (ListValue)target;
        if (listValue.initialValue == null)
        {
            listValue.initialValue = new List<ListValue.ListElement>();
        }
        if (listValue.RuntimeValue == null)
        {
            listValue.RuntimeValue = new List<ListValue.ListElement>();
        }
        UnityEditor.EditorGUILayout.LabelField("Initial Value", UnityEditor.EditorStyles.boldLabel);
        DrawList(listValue.initialValue);
        //Space
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //Space
        UnityEditor.EditorGUILayout.LabelField("Runtime Value", UnityEditor.EditorStyles.boldLabel);
        DrawList(listValue.RuntimeValue);
    
        if (GUI.changed)
        {
            UnityEditor.EditorUtility.SetDirty(target);
        }
    
    }
    
    public void DrawElement(ListValue.ListElement element)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Type");
        element.type = (ListValue.Type)EditorGUILayout.EnumPopup(element.type);
        EditorGUILayout.LabelField("Value");
        switch (element.type)
        {
            case ListValue.Type.Int:
                element.intValue = EditorGUILayout.IntField(element.intValue);
                break;
            case ListValue.Type.Float:
                element.floatValue = EditorGUILayout.FloatField(element.floatValue);
                break;
            case ListValue.Type.Bool:
                element.boolValue = EditorGUILayout.Toggle(element.boolValue);
                break;
            case ListValue.Type.String:
                element.stringValue = EditorGUILayout.TextField(element.stringValue);
                break;
        }
        EditorGUILayout.EndVertical();
    }
    
    public void DrawList(List<ListValue.ListElement> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Element " + i);
            if (GUILayout.Button("Remove"))
            {
                list.RemoveAt(i);
                return;
            }
            EditorGUILayout.EndHorizontal();
            DrawElement(list[i]);
            EditorGUILayout.EndVertical();
            //line
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
        if (GUILayout.Button("Add Element"))
        {
            list.Add(new ListValue.ListElement(ListValue.Type.Int, 0));
        }
    }
}
#endif

