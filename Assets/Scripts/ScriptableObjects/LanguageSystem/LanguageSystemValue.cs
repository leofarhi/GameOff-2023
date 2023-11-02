using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "System Value/LanguageSystemValue")]
[System.Serializable]
public class LanguageSystemValue : ScriptableObject
{
    public static LanguageSystemValue Instance;
    public delegate void LanguageChangeHandler(LanguageSystemValue languageSystemValue);

    public event LanguageChangeHandler OnGameStateChanged;

    [System.Serializable]
    public class Text
    {
        public string uuid;
        public List<string> text;

        public Text()
        {
            text = new List<string>();
            uuid = "";
        }
    }
    [System.Serializable]
    public class ReferenceText
    {
        public string uuid;
        public Text text;
        
        public ReferenceText()
        {
            uuid = "";
            text = null;
        }
        public string GetText(float FontSize =100)
        {
            if (uuid == "")
                text = null;
            LanguageSystemValue languageSystemValue = LanguageSystemValue.Instance;
            if (languageSystemValue==null)
                return "";
            if (text == null || uuid!=text.uuid)
            {
                text = languageSystemValue.GetTextValue(uuid);
            }
            return languageSystemValue.GetText(text, FontSize);
        }
        
        public void SetText(string txt)
        {
            if (uuid == null)
                text = null;
            LanguageSystemValue languageSystemValue = LanguageSystemValue.Instance;
            if (languageSystemValue==null)
                return;
            if (text == null)
            {
                text = languageSystemValue.GetTextValue(uuid);
            }
            int index = languageSystemValue.languagesName.IndexOf(languageSystemValue.currentLanguage);
            if (index==-1)
                index = 0;
            text.text[index] = txt;
        }
    }
    
    public List<string> languagesName = new List<string>();
    public string currentLanguage;
    public List<Text> texts = new List<Text>();
    
    public void ChangeLanguage(string language)
    {
        currentLanguage = language;
        OnGameStateChanged?.Invoke(this);
    }
    
    public Text GetTextValue(string uuid)
    {
        foreach (Text text in texts)
        {
            if (text.uuid == uuid)
                return text;
        }

        return null;
    }
    
    public void SetTextValue(string uuid, Text text)
    {
        foreach (Text txt in texts)
        {
            if (txt.uuid == uuid)
            {
                txt.text = text.text;
                return;
            }
        }
    }

    public string GetText(Text text, float FontSize =100)
    {
        if (text==null)
            return "";
        if (text.text == null)
            text.text = new List<string>();
        if (text.text.Count!=languagesName.Count)
        {
            while (text.text.Count<languagesName.Count)
            {
                text.text.Add("INITIAL_TEXT");
            }
            while (text.text.Count>languagesName.Count)
            {
                text.text.RemoveAt(text.text.Count-1);
            }
        }
        int index = languagesName.IndexOf(currentLanguage);
        if (index==-1)
            index = 0;
        string txt = text.text[index];
        return txt;
    }
    
    public string NewText()
    {
        Text text = new Text();
        text.uuid = System.Guid.NewGuid().ToString();
        texts.Add(text);
        return text.uuid;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LanguageSystemValue.ReferenceText))]
public class ReferenceTextDrawer : PropertyDrawer
{
    private LanguageSystemValue languageSystemValue = null;
    private int index = -1;
    private bool showUUID = false;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (languageSystemValue==null)
        {
            languageSystemValue = (LanguageSystemValue)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/Language System Value.asset", typeof(LanguageSystemValue));
            if (languageSystemValue==null)
            {
                languageSystemValue = ScriptableObject.CreateInstance<LanguageSystemValue>();
                AssetDatabase.CreateAsset(languageSystemValue, "Assets/ScriptableObjects/Language System Value.asset");
                AssetDatabase.SaveAssets();
            }
        }
        
        string uuid = property.FindPropertyRelative("uuid").stringValue;
        index = languageSystemValue.texts.FindIndex(x => x.uuid == uuid);
        if (uuid=="" || index==-1)
        {
            uuid = languageSystemValue.NewText();
            property.FindPropertyRelative("uuid").stringValue = uuid;
            index = languageSystemValue.texts.FindIndex(x => x.uuid == uuid);
        }
        
        var text = languageSystemValue.texts[index];
        if (text.text == null)
            text.text = new List<string>();
        if (text.text.Count!=languageSystemValue.languagesName.Count)
        {
            while (text.text.Count<languageSystemValue.languagesName.Count)
            {
                text.text.Add("INITIAL_TEXT");
            }
            while (text.text.Count>languageSystemValue.languagesName.Count)
            {
                text.text.RemoveAt(text.text.Count-1);
            }
        }

        EditorGUI.BeginProperty(position, label, property);
        //Draw TextField uuid
        Rect uuidRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        GUIContent uuidLabel = new GUIContent("uuid");
        showUUID = EditorGUI.Foldout(uuidRect, showUUID, uuidLabel);
        if (showUUID)
        {
            text.uuid = EditorGUI.TextField(uuidRect, uuidLabel, text.uuid);
        }
        //Draw list text
        Rect listRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, position.width, EditorGUIUtility.singleLineHeight);
        for (int i = 0; i < text.text.Count; i++)
        {
            GUIContent elementLabel = new GUIContent(languageSystemValue.languagesName[i]);
            text.text[i] = EditorGUI.TextField(listRect, elementLabel, text.text[i]);
            listRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
        EditorGUI.EndProperty();

    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty uuidProperty = property.FindPropertyRelative("uuid");
        string uuid = uuidProperty.stringValue;
        languageSystemValue = (LanguageSystemValue)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/Language System Value.asset", typeof(LanguageSystemValue));
        if (languageSystemValue==null)
            return EditorGUIUtility.singleLineHeight;
        index = languageSystemValue.texts.FindIndex(x => x.uuid == uuid);
        if (uuid=="" || index==-1)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        var text = languageSystemValue.texts[index];
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * text.text.Count + EditorGUIUtility.singleLineHeight;
    }
    
}
#endif