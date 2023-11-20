using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/DataSaveValue")]
[System.Serializable]
public class DataSaveValue : SystemValue
{

    public static string folderPath
    {
        get
        {
            return Application.persistentDataPath + "/saves/";
        }
    }
    
    [Serializable]
    public class ExtraData
    {
        public string saveName;
        public List<string> scenesLoaded;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        
        public ExtraData()
        {
            scenesLoaded = new List<string>();
        }
        
        public void Write(BinaryWriter binary)
        {
            binary.Write(saveName);
            if (scenesLoaded == null)
            {
                scenesLoaded = new List<string>();
            }
            binary.Write(scenesLoaded.Count);
            foreach (string scene in scenesLoaded)
            {
                binary.Write(scene);
            }
            binary.Write(playerPosition.x);
            binary.Write(playerPosition.y);
            binary.Write(playerPosition.z);
            binary.Write(playerRotation.x);
            binary.Write(playerRotation.y);
            binary.Write(playerRotation.z);
            binary.Write(playerRotation.w);
        }
        
        public void Read(BinaryReader binary)
        {
            saveName = binary.ReadString();
            int count = binary.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                scenesLoaded.Add(binary.ReadString());
            }
            playerPosition = new Vector3(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
            playerRotation = new Quaternion(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
        }
    }
    public ExtraData initialValue;
    public ExtraData RuntimeValue;
    [Space(10)]
    public List<VariablesValue> variablesValues = new List<VariablesValue>();

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
        RuntimeValue.scenesLoaded = new List<string>(UnityEngine.SceneManagement.SceneManager.GetAllScenes().Select(x => x.name));
        RuntimeValue.Write(binary);
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
        //copy initial value to runtime value
        RuntimeValue.saveName = initialValue.saveName;
        RuntimeValue.playerPosition = initialValue.playerPosition;
        RuntimeValue.playerRotation = initialValue.playerRotation;
        //copy initial value to runtime value
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
            RuntimeValue.Read(binary);
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
