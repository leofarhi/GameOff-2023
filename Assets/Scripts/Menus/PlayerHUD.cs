using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public ThirdPersonController player
    {
        get
        {
            return ThirdPersonController.instance;
        }
    }
    public Image temperatureBar;
    
    //Volume
    public Volume cold;
    public Volume hot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)return;
        temperatureBar.fillAmount = (player.minTemperature - player.currentTemperature) / (player.minTemperature - player.maxTemperature);
        cold.weight = Mathf.Clamp01(1 - (player.minTemperature - player.currentTemperature) / (player.minTemperature - player.temperatureRange.cold));
        hot.weight = Mathf.Clamp01((player.temperatureRange.hot - player.currentTemperature) / (player.temperatureRange.hot - player.maxTemperature));
    }
}
