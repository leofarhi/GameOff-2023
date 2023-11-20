using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD instance;
    public ThirdPersonController player
    {
        get
        {
            return ThirdPersonController.instance;
        }
    }
    
    public GameObject mainPanel;
    [Header("Temperature")]
    public Image temperatureBar;
    
    //Volume
    public Volume cold;
    public Volume hot;
    [Header("System")]
    public GameObject interactionIcon;

    
    public bool interactionIconActive
    {
        get
        {
            return interactionWith != null;
        }
    }
    [HideInInspector]
    public object interactionWith = null;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)return;
        
        temperatureBar.fillAmount = (player.minTemperature - player.currentTemperature) / (player.minTemperature - player.maxTemperature);
        cold.weight = Mathf.Clamp01(1 - (player.minTemperature - player.currentTemperature) / (player.minTemperature - player.temperatureRange.cold));
        hot.weight = Mathf.Clamp01((player.temperatureRange.hot - player.currentTemperature) / (player.temperatureRange.hot - player.maxTemperature));
        
        if (interactionIcon.activeSelf != interactionIconActive)
            interactionIcon.SetActive(interactionIconActive);
    }
    
    public void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState == GameState.Gameplay)
        {
            mainPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }
        
    private void OnDisable()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
}
