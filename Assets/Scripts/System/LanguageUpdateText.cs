using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LanguageUpdateText : MonoBehaviour
{
    public LanguageSystemValue languageSystemValue
    {
        get
        {
            return LanguageSystemValue.Instance;
        }
    }
    public TMPro.TMP_Text text;
    public LanguageSystemValue.ReferenceText textValue;


    public void OnLanguageChange(LanguageSystemValue languageSystemValue)
    {
        text.text = textValue.GetText(text.fontSize);
    }

    public void Start()
    {
        if (text == null)
            text = GetComponent<TMPro.TMP_Text>();
        OnLanguageChange(languageSystemValue);
    }

    public void OnEnable()
    {
        OnLanguageChange(languageSystemValue);
        if (languageSystemValue!=null)
            languageSystemValue.OnGameStateChanged += OnLanguageChange;
    }
    
    public void OnDisable()
    {
        if (languageSystemValue!=null)
            languageSystemValue.OnGameStateChanged -= OnLanguageChange;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LanguageUpdateText))]
public class LanguageUpdateTextEditor : Editor
{
    //On add component, first time, set the text value to the first text value in the language system value
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LanguageUpdateText languageUpdateText = (LanguageUpdateText) target;
        LanguageSystemValue languageSystemValue = (LanguageSystemValue)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/Language System Value.asset", typeof(LanguageSystemValue));
        LanguageSystemValue.Text text = languageSystemValue.GetTextValue(languageUpdateText.textValue.uuid);
        if (text == null && languageUpdateText.textValue.uuid != "")
        {
            languageUpdateText.textValue.uuid = "";
            return;
        }
        if (languageUpdateText.text!=null && text.text.Count>0 && text.text[0]=="INITIAL_TEXT")
        {
            for (int i = 0; i < text.text.Count; i++)
            {
                text.text[i] = languageUpdateText.text.text;
            }
            languageSystemValue.SetTextValue(languageUpdateText.textValue.uuid, text);
        }
    }
    
}
#endif