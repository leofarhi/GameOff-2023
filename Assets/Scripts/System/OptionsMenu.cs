using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    #region ButtonClass
    [Serializable]
    public class ButtonName
    {
        public enum ButtonType
        {
            Button,
            Slider,
            Swipe,
            Dropdown
        }
        public string name;
        public GameObject button;
        public ButtonType buttonType;
        
        public List<LanguageSystemValue.ReferenceText> options;
        [HideInInspector]
        public string uuid = "";
        
        public int currentOption;
        public delegate void ButtonHandler(ButtonName buttonName);
        public event ButtonHandler OnButtonPressed;

        public void Invoke()
        {
            OnButtonPressed?.Invoke(this);
        }
        
        public TMPro.TMP_Text GetText()
        {
            TMPro.TMP_Text text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            if (text != null && currentOption < options.Count && currentOption >= 0)
            {
                text.text = options[currentOption].GetText(text.fontSize);
            }
            return text;
        }

        public TMPro.TMP_Dropdown GetDropdown()
        {
            TMPro.TMP_Dropdown dropdown = button.GetComponentInChildren<TMPro.TMP_Dropdown>();
            if (dropdown != null)
            {
                dropdown.value = currentOption;
            }
            return dropdown;
        }
        
        public UnityEngine.UI.Slider GetSlider()
        {
            UnityEngine.UI.Slider slider = button.GetComponentInChildren<UnityEngine.UI.Slider>();
            if (slider != null)
            {
                slider.value = currentOption;
            }
            return slider;
        }

        public void SetOption(int option)
        {
            if (option < 0)
                return;
            currentOption = option;
            switch (buttonType)
            {
                case ButtonType.Button:
                    GetText();
                    break;
                case ButtonType.Slider:
                    GetSlider();
                    break;
                case ButtonType.Swipe:
                    GetText();
                    break;
                case ButtonType.Dropdown:
                    GetDropdown();
                    break;
            }
        }
        
        public void Enable()
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                    button.GetComponent<UnityEngine.UI.Button>().interactable = true;
                    break;
                case ButtonType.Slider:
                    button.GetComponent<UnityEngine.UI.Slider>().interactable = true;
                    break;
                case ButtonType.Swipe:
                    button.GetComponent<UnityEngine.UI.Button>().interactable = true;
                    break;
                case ButtonType.Dropdown:
                    button.GetComponent<TMPro.TMP_Dropdown>().interactable = true;
                    break;
            }
        }
        
        public void Disable()
        {
            switch (buttonType)
            {
                case ButtonType.Button:
                    button.GetComponent<UnityEngine.UI.Button>().interactable = false;
                    break;
                case ButtonType.Slider:
                    button.GetComponent<UnityEngine.UI.Slider>().interactable = false;
                    break;
                case ButtonType.Swipe:
                    button.GetComponent<UnityEngine.UI.Button>().interactable = false;
                    break;
                case ButtonType.Dropdown:
                    button.GetComponent<TMPro.TMP_Dropdown>().interactable = false;
                    break;
            }
        }
    }
    #endregion
    
    public SettingsValue settingsValue;
    public LanguageSystemValue languageSystemValue;
    public InputPreset mainInputPreset;
    public static OptionsMenu Instance;


    [Header("Panels")]
    public GameObject infoPanel;
    public GameObject KeyBindingPanel;
    
    [Header("Buttons")]
    public List<ButtonName> buttons = new List<ButtonName>();
    public List<ButtonName> buttonsKeyBinding = new List<ButtonName>();
    [SerializeField]
    public List<string> buttonDisabledWithController = new List<string>();

    public void Swipe(ButtonName buttonName)
    {
        buttonName.currentOption++;
        if (buttonName.currentOption >= buttonName.options.Count)
            buttonName.currentOption = 0;

        TMPro.TMP_Text text = buttonName.button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
        if (text != null)
        {
            text.text = buttonName.options[buttonName.currentOption].GetText(text.fontSize);
        }
    }
    
#region DetectInput
    
    private Event keyEvent = null;
    private bool waiting = false;
    void OnGUI()
    {
        keyEvent = Event.current;
    }

    IEnumerator WaitForKey(string NameInput)
    {
        GenericInput input = mainInputPreset.GetInput(NameInput);
        if (input==null)
            yield break;
        waiting = true;
        while (waiting)
        {
            if (keyEvent != null && keyEvent.isKey && keyEvent.keyCode != KeyCode.None)
            {
                Debug.Log("Detected key code: " + keyEvent.keyCode);
                input.keyboard = keyEvent.keyCode.ToString();
                mainInputPreset.SetInput(NameInput, input);
                waiting = false;
                vInput.instance.inputDevice = InputDevice.MouseKeyboard;
            }
            else 
            {
                string[] axis = new string[] {"LeftAnalogHorizontal", "LeftAnalogVertical", "RightAnalogHorizontal", "RightAnalogVertical","LeftStickClick","RightStickClick", "LT", "RT", "D-Pad Horizontal", "D-Pad Vertical"};
                foreach (string s in axis)
                {
                    if (Input.GetAxis(s) != 0.0f)
                    {
                        Debug.Log("Detected key code: " + s);
                        input.joystick = s;
                        mainInputPreset.SetInput(NameInput, input);
                        waiting = false;
                        vInput.instance.inputDevice = InputDevice.Joystick;
                        break;
                    }
                }
                string[] buttons = new string[] {"A", "B", "X", "Y", "LB", "RB", "Back", "Start", "LeftStickClick", "RightStickClick"};
                foreach (string s in buttons)
                {
                    if (Input.GetButtonDown(s))
                    {
                        Debug.Log("Detected key code: " + s);
                        input.joystick = s;
                        mainInputPreset.SetInput(NameInput, input);
                        waiting = false;
                        vInput.instance.inputDevice = InputDevice.Joystick;
                        break;
                    }
                }
            }
            yield return null;
        }
        keyEvent = null;
        waiting = false;
        CancelKeyBinding();
    }
    
    public void CancelKeyBinding()
    {
        KeyBindingPanel.SetActive(false);
        infoPanel.SetActive(false);
        waiting = false;
        UpdateKeyButtons();
    }
    
    public void StartKeyBinding(string input)
    {
        KeyBindingPanel.SetActive(true);
        infoPanel.SetActive(true);
        StartCoroutine(WaitForKey(input));
    }
#endregion

    public void Start()
    {
        Instance = this;
        List<ButtonName> merge = new List<ButtonName>();
        merge.AddRange(buttons);
        merge.AddRange(buttonsKeyBinding);
        foreach (ButtonName button in merge)
        {
            switch (button.buttonType)
            {
                case ButtonName.ButtonType.Button:
                    button.button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => button.Invoke());
                    break;
                case ButtonName.ButtonType.Swipe:
                    button.button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => button.Invoke());
                    button.OnButtonPressed += Swipe;
                    break;
                case ButtonName.ButtonType.Slider:
                    button.button.GetComponent<UnityEngine.UI.Slider>().onValueChanged.AddListener((value) =>
                    {
                        button.currentOption = (int)value;
                        button.Invoke();
                    });
                    break;
                case ButtonName.ButtonType.Dropdown:
                    button.button.GetComponent<TMPro.TMP_Dropdown>().onValueChanged.AddListener((value) =>
                    {
                        button.currentOption = value;
                        button.Invoke();
                    });
                    break;
                
            }
        }
        SetSettingButton();
        UpdateLanguage();
    }
    
    public ButtonName GetButton(string name)
    {
        List<ButtonName> merge = new List<ButtonName>();
        merge.AddRange(buttons);
        merge.AddRange(buttonsKeyBinding);
        foreach (ButtonName button in merge)
        {
            if (button.name.ToLower() == name.ToLower())
                return button;
        }
        return null;
    }

    public void SetSettingButton()
    {
        ButtonName languageButton = GetButton("Language");
        int OptionIndex = languageSystemValue.languagesName.IndexOf(languageSystemValue.currentLanguage);
        languageButton.SetOption(OptionIndex);
        languageButton.OnButtonPressed += (buttonName) =>
        {
            languageSystemValue.ChangeLanguage(languageSystemValue.languagesName[buttonName.currentOption]);
        };

        ButtonName resolutionButton = GetButton("Resolution");
        TMPro.TMP_Dropdown dropdown = resolutionButton.GetDropdown();
        dropdown.ClearOptions();
        //remove duplicate
        List<string> resolutionsList = new List<string>();
        foreach (Resolution res in Screen.resolutions)
        {
            string resolution = res.width + "x" + res.height;
            if (!resolutionsList.Contains(resolution))
            {
                resolutionsList.Add(resolution);
                dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(resolution));
            }
        }
        OptionIndex = dropdown.options.FindIndex((option) =>
        {
            string[] split = option.text.Split('x');
            Vector2Int resolution = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));
            return resolution == settingsValue.resolution;
        });
        resolutionButton.SetOption(OptionIndex);
        resolutionButton.OnButtonPressed += (buttonName) =>
        {
            TMPro.TMP_Dropdown dropdown = buttonName.GetDropdown();
            string[] split = dropdown.options[dropdown.value].text.Split('x');
            buttonName.currentOption = dropdown.value;
            Vector2Int resolution = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));
            settingsValue.SetResolution(resolution);
        };

        ButtonName fullscreenButton = GetButton("Fullscreen");
        OptionIndex = settingsValue.fullscreen ? 0 : 1;
        fullscreenButton.SetOption(OptionIndex);
        fullscreenButton.OnButtonPressed += (buttonName) =>
        {
            settingsValue.SetFullscreen(buttonName.currentOption == 0);
        };

        ButtonName controllerButton = GetButton("Controller");
        OptionIndex = (int)settingsValue.controller;
        controllerButton.SetOption(OptionIndex);
        controllerButton.OnButtonPressed += (buttonName) =>
        {
            TMPro.TMP_Dropdown dropdown = buttonName.GetDropdown();
            buttonName.currentOption = dropdown.value;
            settingsValue.SetController(dropdown.value);
        };

        ButtonName mainVolume = GetButton("MainVolume");
        mainVolume.GetSlider().value = settingsValue.generalVolume*100;
        mainVolume.OnButtonPressed += (buttonName) =>
        {
            settingsValue.SetGeneralVolume((float)buttonName.GetSlider().value/100.0f);
        };

        ButtonName musicVolume = GetButton("MusicVolume");
        musicVolume.GetSlider().value = settingsValue.musicVolume*100;
        musicVolume.OnButtonPressed += (buttonName) =>
        {
            settingsValue.SetMusicVolume((float)buttonName.GetSlider().value/100.0f);
        };

        ButtonName soundVolume = GetButton("SoundVolume");
        soundVolume.GetSlider().value = settingsValue.sfxVolume*100;
        soundVolume.OnButtonPressed += (buttonName) =>
        {
            settingsValue.SetSfxVolume((float)buttonName.GetSlider().value/100.0f);
        };


        UpdateKeyButtons();
        foreach (ButtonName button in buttonsKeyBinding)
        {
            button.OnButtonPressed += (buttonName) =>
            {
                StartKeyBinding(buttonName.name);
            };
        }
    }
    
    
    public void UpdateKeyButtons()
    {
        foreach (ButtonName button in buttonsKeyBinding)
        {
            GenericInput input = mainInputPreset.GetInput(button.name);
            if (input!=null)
            {
                button.GetText().text = input.buttonName;
            }
        }
        foreach (var name in buttonDisabledWithController)
        {
            ButtonName button = GetButton(name);
            if (button != null)
            {
                if (vInput.instance.inputDevice == InputDevice.Joystick)
                {
                    button.Disable();
                }
                else
                {
                    button.Enable();
                }
                if (button.name.Contains("/negative"))
                    button.button.SetActive(vInput.instance.inputDevice != InputDevice.Joystick);
            }
        }
    }
    
    
    
    
    public void OnLanguageChange(LanguageSystemValue languageSystemValue)
    {
        if (Instance!=this) return;
        foreach (ButtonName button in buttons)
        {
            TMPro.TMP_Text text = button.button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            if (text != null && button.options.Count>button.currentOption)
            {
                text.text = button.options[button.currentOption].GetText(text.fontSize);
            }
        }
    }
    
    public void SaveSettings()
    {
        settingsValue.Save();
    }
    
    public void UpdateLanguage()
    {
        OnLanguageChange(languageSystemValue);
    }

    public void OnEnable()
    {
        languageSystemValue.OnGameStateChanged += OnLanguageChange;
        vInput.instance.onChangeInputType += OnChangeInputType;
    }
    
    private void OnChangeInputType(InputDevice type)
    {
        if (Instance!=this) return;
        UpdateKeyButtons();
    }
    
    public void OnDisable()
    {
        languageSystemValue.OnGameStateChanged -= OnLanguageChange;
        //vInput.instance.onChangeInputType -= OnChangeInputType;
    }
    
}
