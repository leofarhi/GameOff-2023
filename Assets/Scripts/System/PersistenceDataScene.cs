using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InterfaceType
{
    None,
    Pause,
    Animation,
    Dialogue,
    Loading,
}


public class PersistenceDataScene : MonoBehaviour
{
    public static PersistenceDataScene Instance;
    public List<GameObject> objectsToPersist = new List<GameObject>();
    public GameObject eventSystem;
    
    public LanguageSystemValue languageSystemValue;
    public SettingsValue settingsValue;
    public InputPreset inputPreset;
    public DataSaveValue dataSaveValue;
    
    public GameObject loadingScreenPanel;
    
    private InterfaceType _interfaceIsOpen;
    
    public ThirdPersonController player
    {
        get
        {
            return ThirdPersonController.instance;
        }
    }

    public InterfaceType InterfaceIsOpen
    {
        get { return _interfaceIsOpen; }
        set
        {
            _interfaceIsOpen = value;
            GameStateManager.Instance.SetState(_interfaceIsOpen == InterfaceType.None
                ? GameState.Gameplay
                : GameState.Paused);
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
        InterfaceIsOpen = InterfaceType.None;
        InputPreset.current = inputPreset;
        LanguageSystemValue.Instance = languageSystemValue;
    }
    
    void Start()
    {
        settingsValue.Load();
    }
    
    private IEnumerator LoadSceneAsync(AsyncOperation operation,bool autoHideLoadingScreen=true)
    {
        InterfaceIsOpen = InterfaceType.Loading;
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
        InterfaceIsOpen = InterfaceType.None;
    }
    
    public void LoadScene(string sceneName,bool autoHideLoadingScreen=true)
    {
        StartCoroutine(LoadSceneAsync(SceneManager.LoadSceneAsync(sceneName),autoHideLoadingScreen));
    }
    
    public void HideLoadingScreen()
    {
        loadingScreenPanel.SetActive(false);
    }
}
