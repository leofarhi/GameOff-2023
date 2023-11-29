using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class PersistenceDataScene : MonoBehaviour
{
    public static PersistenceDataScene Instance;
    public List<GameObject> objectsToPersist = new List<GameObject>();
    public GameObject eventSystem;
    [Space]
    public LanguageSystemValue languageSystemValue;
    public SettingsValue settingsValue;
    public InputPreset inputPreset;
    public DataSaveValue dataSaveValue;
    [Space]
    public BoolValue playerCanTeleport;
    public ListValue capsuleMetRuntime;
    [Space]
    public GameObject loadingScreenPanel;
    public GameObject savePanel;
    public GameObject teleportBox;
    [Space]
    public DialogueInterface dialogueInterface;
    
    
    public ThirdPersonController player
    {
        get
        {
            return ThirdPersonController.instance;
        }
    }

    public GameState InterfaceIsOpen
    {
        get { return GameStateManager.Instance.CurrentGameState; }
        set
        {
            GameStateManager.Instance.SetState(value);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            if (eventSystem != null)
            DontDestroyOnLoad(eventSystem);
            foreach (var obj in objectsToPersist)
            {
                DontDestroyOnLoad(obj);
            }
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy (gameObject);
            return;
        }
        loadingScreenPanel.SetActive(false);
        savePanel.SetActive(false);
        InterfaceIsOpen = GameState.Gameplay;
        InputPreset.current = inputPreset;
        LanguageSystemValue.Instance = languageSystemValue;
    }
    
    void Start()
    {
        settingsValue.Load();
    }
    
    private IEnumerator LoadSceneAsync(AsyncOperation operation,bool autoHideLoadingScreen=true)
    {
        InterfaceIsOpen = GameState.Loading;
        loadingScreenPanel.SetActive(true);
        while (operation!=null && !operation.isDone && loadingScreenPanel.activeSelf)
        {
            yield return null;
        }

        while ((!autoHideLoadingScreen || player == null) && loadingScreenPanel.activeSelf)
        {
            yield return null;
        }
        loadingScreenPanel.SetActive(false);
        InterfaceIsOpen = GameState.Gameplay;
    }
    
    public void LoadScene(string sceneName,bool autoHideLoadingScreen=true)
    {
        StartCoroutine(LoadSceneAsync(SceneManager.LoadSceneAsync(sceneName),autoHideLoadingScreen));
    }
    
    public void HideLoadingScreen()
    {
        loadingScreenPanel.SetActive(false);
    }
    
    private IEnumerator LaunchDialogueEvent(DialogueValue dialogueValue,UnityEvent end=null)
    {
        GameState previousGameState = InterfaceIsOpen;
        InterfaceIsOpen = GameState.Dialogue;
        yield return StartCoroutine(DialogueValue.LaunchDialogue(dialogueValue));
        InterfaceIsOpen = previousGameState;
        end?.Invoke();
    }
    
    public void LaunchDialogue(DialogueValue dialogueValue,UnityEvent end=null)
    {
        StartCoroutine(LaunchDialogueEvent(dialogueValue,end));
    }
    
    public void SaveGame()
    {
        dataSaveValue.Save(dataSaveValue.RuntimeValue.saveName);
        OpenOrCloseSavePanel(false);
    }
    
    public void SaveAndQuit()
    {
        SaveGame();
        ReturnToMainMenu();
        OpenOrCloseSavePanel(false);
    }
    
    public void ReturnToMainMenu()
    {
        dataSaveValue.Reset();
        LoadScene("MainMenu");
        OpenOrCloseSavePanel(false);
    }
    
    public void OpenOrCloseSavePanel(bool open)
    {
        savePanel.SetActive(open);
        if (open)
        {
            InterfaceIsOpen = GameState.Paused;
            teleportBox.SetActive(playerCanTeleport.RuntimeValue);
        }
        else
        {
            InterfaceIsOpen = GameState.Gameplay;
        }
    }
}
