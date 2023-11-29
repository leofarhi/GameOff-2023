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
    public GameObject MainMenuPanel;
    public GameObject LoadGameMenuPanel;
    public GameObject loadGameBox;
    private List<SaveLoadStateMenu> loadGameBoxInstances = new List<SaveLoadStateMenu>();
    private List<string> saveNames = new List<string>();

    private void Start()
    {
        persistenceDataScene.dataSaveValue.Reset();
        persistenceDataScene.HideLoadingScreen();
        persistenceDataScene.InterfaceIsOpen = GameState.Paused;
        loadGameBox.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void UpdateSaves()
    {
        foreach (SaveLoadStateMenu obj in loadGameBoxInstances)
        {
            Destroy(obj.gameObject);
        }
        loadGameBoxInstances.Clear();
        saveNames.Clear();
        string[] files = System.IO.Directory.GetFiles(DataSaveValue.folderPath);
        foreach (string file in files)
        {
            if (file.EndsWith(".save"))
            {
                saveNames.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }
        for (int i = 0; i < saveNames.Count; i++)
        {
            GameObject obj = Instantiate(loadGameBox, loadGameBox.transform.parent);
            obj.SetActive(true);
            SaveLoadStateMenu saveLoadStateMenu = obj.GetComponent<SaveLoadStateMenu>();
            SetupGameBox(saveLoadStateMenu,saveNames[i]);
            loadGameBoxInstances.Add(saveLoadStateMenu);
        }
    }

    public void SetupGameBox(SaveLoadStateMenu saveLoadStateMenu,string saveName)
    {
        saveLoadStateMenu.saveText.text = saveName;
        saveLoadStateMenu.LoadButton.onClick.AddListener(delegate { LoadGameButton(saveName); });
        saveLoadStateMenu.DeleteButton.onClick.AddListener(delegate { DeleteGameButton(saveName);
            UpdateSaves();
        });
    }
    
    public void PlayButton()
    {
        UpdateSaves();
        if (saveNames.Count == 0)
        {
            NewGameButton();
        }
        else
        {
            LoadGameMenuPanel.SetActive(true);
            MainMenuPanel.SetActive(false);
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
