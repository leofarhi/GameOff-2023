using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/DataSaveValue")]
[System.Serializable]
public class DataSaveValue : SystemValue
{
    public List<VariablesValue> variablesValues = new List<VariablesValue>();
    
    public static string folderPath = Application.persistentDataPath + "/saves/";
    
    [Serializable]
    public struct ExtraData
    {
        public List<string> scenesLoaded;
    }
    public ExtraData initialValue;
    public ExtraData RuntimeValue;

    public void Save(string saveName)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        string path = folderPath + saveName + ".save";
        FileStream file  = new FileStream(path, FileMode.OpenOrCreate);
        BinaryWriter binary = new BinaryWriter(file);
        //Save extra data
        if (RuntimeValue.scenesLoaded == null)
        {
            RuntimeValue.scenesLoaded = new List<string>();
        }
        binary.Write(RuntimeValue.scenesLoaded.Count);
        foreach (string scene in RuntimeValue.scenesLoaded)
        {
            binary.Write(scene);
        }
        //Save variables
        foreach (VariablesValue value in variablesValues)
        {
            value.SaveValue(binary);
        }
        binary.Close();
        file.Close();
    }
    
    public void Reset()
    {
        if (initialValue.scenesLoaded == null)
        {
            initialValue.scenesLoaded = new List<string>();
        }
        //copy initial value to runtime value
        RuntimeValue.scenesLoaded = new List<string>(initialValue.scenesLoaded);
        foreach (VariablesValue value in variablesValues)
        {
            value.Reset();
        }
    }

    public void Load(string saveName)
    {
        string path = folderPath + saveName + ".save";
        if (!File.Exists(path))
        {
            Reset();
            Save(saveName);
        }
        else
        {
            FileStream file  = new FileStream(path, FileMode.Open);
            BinaryReader binary = new BinaryReader(file);
            //Load extra data
            int scenesLoadedCount = binary.ReadInt32();
            RuntimeValue.scenesLoaded = new List<string>();
            for (int i = 0; i < scenesLoadedCount; i++)
            {
                RuntimeValue.scenesLoaded.Add(binary.ReadString());
            }
            //Load variables
            foreach (VariablesValue value in variablesValues)
            {
                value.LoadValue(binary);
            }
            binary.Close();
            file.Close();
        }
        //Load scenes
        foreach (string scene in RuntimeValue.scenesLoaded)
        {
            //Load First Scene
            if (scene == RuntimeValue.scenesLoaded[0])
            {
                PersistenceDataScene.Instance.LoadScene(scene);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
        }
    }
    
    public void NewSave(string saveName)
    {
        Reset();
        Save(saveName);
    }
}
