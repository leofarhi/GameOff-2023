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
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public float gameOverDelay = 1f;
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
    
    public void CloseAllPanels()
    {
        OpenOrCloseSavePanel(false);
        OpenOrClosePauseMenu(false);
        gameOverPanel.SetActive(false);
    }
    
    public void SaveAndQuit()
    {
        SaveGame();
        ReturnToMainMenu();
        CloseAllPanels();
    }
    
    public void ReturnToMainMenu()
    {
        dataSaveValue.Reset();
        CloseAllPanels();
        LoadScene("MainMenu");
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
    
    public void OpenOrClosePauseMenu(bool open)
    {
        pauseMenuPanel.SetActive(open);
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

    public void Update()
    {
        if (InputPreset.current.pauseInput.GetButtonDown())
        {
            if (InterfaceIsOpen == GameState.Paused)
            {
                CloseAllPanels();
            }
            else if (InterfaceIsOpen == GameState.Gameplay)
            {
                OpenOrClosePauseMenu(true);
            }
        }
    }
    
    public void GameOver()
    {
        StartCoroutine(GameOverEvent());
    }
    
    public void ReloadGame()
    {
        CloseAllPanels();
        dataSaveValue.Load(dataSaveValue.RuntimeValue.saveName);
    }
    
    private IEnumerator GameOverEvent()
    {
        InterfaceIsOpen = GameState.Animation;
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(gameOverDelay);
        InterfaceIsOpen = GameState.Dialogue;
        //Load empty scene
        SceneManager.LoadScene("EmptyScene");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
        }

    }
}
