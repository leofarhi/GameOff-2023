using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public PersistenceDataScene persistenceDataScene
    {
        get { return PersistenceDataScene.Instance; }
    }
    
    public GameObject loadGameBox;
    private List<GameObject> loadGameBoxInstances = new List<GameObject>();
    private List<string> saveNames = new List<string>();

    private void Start()
    {
        persistenceDataScene.dataSaveValue.Reset();
        persistenceDataScene.HideLoadingScreen();
        persistenceDataScene.InterfaceIsOpen = InterfaceType.Pause;
        loadGameBox.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void UpdateSaves()
    {
        foreach (GameObject obj in loadGameBoxInstances)
        {
            Destroy(obj);
        }
        loadGameBoxInstances.Clear();
        saveNames.Clear();
        string[] files = System.IO.Directory.GetFiles(DataSaveValue.folderPath);
        foreach (string file in files)
        {
            if (file.EndsWith(".save"))
            {
                string[] split = file.Split('\\');
                string[] split2 = split[split.Length - 1].Split('.');
                saveNames.Add(split2[0]);
            }
        }
        for (int i = 0; i < saveNames.Count; i++)
        {
            GameObject obj = Instantiate(loadGameBox, loadGameBox.transform.parent);
            obj.SetActive(true);
            loadGameBoxInstances.Add(obj);
        }
    }
    
    public void PlayButton()
    {
        UpdateSaves();
        if (saveNames.Count == 0)
        {
            NewGameButton();
        }
    }
    
    public void NewGameButton()
    {
        UpdateSaves();
        string saveName;
        int i = 0;
        do
        {
            saveName = "save" + i;
            i++;
        } while (saveNames.Contains(saveName));
        persistenceDataScene.dataSaveValue.NewSave(saveName);
        persistenceDataScene.dataSaveValue.Load(saveName);
    }
    
    public void LoadGameButton(string saveName)
    {
        persistenceDataScene.dataSaveValue.Load(saveName);
    }
    
    public void DeleteGameButton(string saveName)
    {
        if (System.IO.File.Exists(DataSaveValue.folderPath + saveName + ".save"))
        {
            System.IO.File.Delete(DataSaveValue.folderPath + saveName + ".save");
        }
        if (System.IO.File.Exists(DataSaveValue.folderPath + saveName + ".png"))
        {
            System.IO.File.Delete(DataSaveValue.folderPath + saveName + ".png");
        }
    }

    
    

}
