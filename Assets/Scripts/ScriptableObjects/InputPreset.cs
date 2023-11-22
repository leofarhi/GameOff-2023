using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/InputPreset")]
[System.Serializable]
public class InputPreset : ScriptableObject
{
    public static InputPreset current;
    
    public GenericInput pauseInput = new GenericInput("Escape", "Start", "Start");
    [Header("Movement Input")]
    public GenericAxis horizontalInput = new GenericAxis(new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal"));
    public GenericAxis verticalInput = new GenericAxis(new GenericInput("Vertical", "LeftAnalogVertical", "Vertical"));
    public Vector2 moveInput
    {
        get
        {
            return new Vector2(horizontalInput.GetAxisRaw(), verticalInput.GetAxisRaw());
        }
    }
    
    public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
    public GenericInput jumpInput = new GenericInput("Space", "X", "X");
    public GenericInput actionInput = new GenericInput("E", "B", "B");
    public GenericInput cancelInput = new GenericInput("Q", "A", "A");
    public GenericInput attackInput = new GenericInput("Mouse0", "RightTrigger", "RightTrigger");
    
    public GenericInput GetInput(string name)
        {
            string[] names = name.Split('/');
            var input = GetType().GetField(names[0]).GetValue(this);
            if (input is GenericAxis)
            {
                if (names.Length > 1)
                {
                    if (names[1].ToLower() == "positive")
                    {
                        return ((GenericAxis)input).positiveButton;
                    }
                    else if (names[1].ToLower() == "negative")
                    {
                        return ((GenericAxis)input).negativeButton;
                    }
                }
            }
            else
            {
                return (GenericInput)input;
            }
            return null;
        }
        
        public void SetInput(string name, GenericInput value)
        {
            string[] names = name.Split('/');
            if (names.Length > 1)
            {
                var input = GetType().GetField(names[0]).GetValue(this);
                if (input is GenericAxis)
                {
                    if (names[1].ToLower() == "positive")
                    {
                        ((GenericAxis) input).positiveButton = value;
                    }
                    else if (names[1].ToLower() == "negative")
                    {
                        ((GenericAxis) input).negativeButton = value;
                    }
                }
            }
            else
            {
                GetType().GetField(name).SetValue(this, value);
            }
        }
        
        public void Save()
        {
            string path = Application.persistentDataPath + "/input.json";
            //save in json format
            var json = JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(path, json);
        }
        
        public void Load()
        {
            string path = Application.persistentDataPath + "/input.json";
            //load from json format
            if (!System.IO.File.Exists(path))
            {
                Save();
                return;
            }
            var json = System.IO.File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
        }
}
