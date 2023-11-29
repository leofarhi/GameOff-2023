using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "System Value/SettingsValue")]
[System.Serializable]
public class SettingsValue : ScriptableObject
{
    public enum Controller
    {
        Auto,
        KeyboardMouse,
        Controller
    }
    public InputPreset mainInputPreset
    {
        get { return InputPreset.current; }
    }

    public LanguageSystemValue languageSystemValue
    {
        get { return LanguageSystemValue.Instance; }
    }
    public AudioMixer audioMixer;
    
    public double versionSettings = 1.01f;
    public string language;
    public bool vibration;
    public Vector2Int resolution;
    public int quality;
    public bool fullscreen;
    public int controllerDisplay;
    public float generalVolume;
    public float musicVolume;
    public float sfxVolume;
    public Controller controller;
    
    public void SetResolution(Vector2Int resolution)
    {
        this.resolution = resolution;
        Screen.SetResolution(resolution.x, resolution.y, fullscreen);
    }
    
    public void SetQuality(int quality)
    {
        this.quality = quality;
        QualitySettings.SetQualityLevel(quality);
    }
    
    public void SetFullscreen(bool fullscreen)
    {
        this.fullscreen = fullscreen;
        Screen.SetResolution(resolution.x, resolution.y, fullscreen);
    }

    public void SetGeneralVolume(float generalVolume)
    {
        this.generalVolume = generalVolume;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("fm_volume_master", generalVolume);
        if (generalVolume == 0)
            this.audioMixer.SetFloat("MasterVolume", -80);
        else
            this.audioMixer.SetFloat("MasterVolume", Mathf.Log10(generalVolume) * 20);
    }
    
    public void SetMusicVolume(float musicVolume)
    {
        this.musicVolume = musicVolume;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("fm_volume_mx", musicVolume);
        if (musicVolume == 0)
            this.audioMixer.SetFloat("MusicVolume", -80);
        else
            this.audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
    }
    
    public void SetSfxVolume(float sfxVolume)
    {
        this.sfxVolume = sfxVolume;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("fm_volume_sfx", sfxVolume);
        if (sfxVolume == 0)
            this.audioMixer.SetFloat("SoundsVolume", -80);
        else
            this.audioMixer.SetFloat("SoundsVolume", Mathf.Log10(sfxVolume) * 20);
    }

    public void SetController(int controller)
    {
        this.controller = (Controller) controller;
    }
    
    public void SetDefault()
    {
        if (LanguageSystemValue.Instance==null)
            this.language = "Français";
        else
            this.language = LanguageSystemValue.Instance.languagesName[0];
        SetResolution(new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height));
        SetQuality(QualitySettings.GetQualityLevel());
        SetFullscreen(Screen.fullScreen);
        SetGeneralVolume(1);
        SetMusicVolume(1);
        SetSfxVolume(1);
        SetController(0);
    }
    
    public void Apply()
    {
        LanguageSystemValue.Instance.ChangeLanguage(language);
        SetResolution(resolution);
        SetQuality(quality);
        SetFullscreen(fullscreen);
        SetGeneralVolume(generalVolume);
        SetMusicVolume(musicVolume);
        SetSfxVolume(sfxVolume);
        SetController((int)controller);
    }
    public void Save()
    {
        string path = Application.persistentDataPath + "/settings.json";
        mainInputPreset.Save();
        //Write to binary file
        Debug.Log(path);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
        var json = JsonUtility.ToJson(this, true);
        System.IO.File.WriteAllText(path, json);
    }

    public void Load()
    {
        string path = Application.persistentDataPath + "/settings.json";
        mainInputPreset.Load();
        Debug.Log(path);
        //Read from binary file
        if (!System.IO.File.Exists(path))
        {
            SetDefault();
            Save();
            return;
        }
        var json = System.IO.File.ReadAllText(path);
        //read versionSettings key
        var dynamicObject = Newtonsoft.Json.Linq.JObject.Parse(json);
        double version = (double)dynamicObject["versionSettings"];
        if (version != versionSettings)
        {
            Debug.LogError("Settings file version is not compatible");
            SetDefault();
            Save();
            return;
        }
        AudioMixer temp = audioMixer;
        JsonUtility.FromJsonOverwrite(json, this);
        audioMixer = temp;
        Apply();
        if (OptionsMenu.Instance != null)
            OptionsMenu.Instance.UpdateLanguage();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(SettingsValue))]
public class SettingsValueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //Add Update button
        SettingsValue settingsValueValue = (SettingsValue)target;
        if (GUILayout.Button("Update"))
        {
            //Update all settings
            settingsValueValue.Apply();
        }
    }
}
#endif