using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public ThirdPersonController player
    {
        get
        {
            return ThirdPersonController.instance;
        }
    }
    public TMPro.TextMeshProUGUI temperatureText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)return;
        temperatureText.text = player.currentTemperature.ToString("0.0") + "°C";
    }
}
