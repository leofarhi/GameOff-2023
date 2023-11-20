using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "System Value/Dialogues/DialogueTextValue")]
public class DialogueTextValue : DialogueValue
{
    [Serializable]
    public class Dialogue
    {
        public Sprite face;
        public string name;
        public List<string> text = new List<string>();
    }
    [HideInInspector]
    public List<Dialogue> texts = new List<Dialogue>();

    public string GetText(int index, float FontSize =100)
    {
        if (texts == null)
            return "";
        if (texts.Count <= index)
            return "";
        LanguageSystemValue languageSystemValue = LanguageSystemValue.Instance;
        if (languageSystemValue==null)
            return "";
        int indexLang = languageSystemValue.languagesName.IndexOf(languageSystemValue.currentLanguage);
        return texts[index].text[indexLang];
    }
    
    public Tuple<string,Sprite> GetCharacter(int index)
    {
        if (texts == null)
            return new Tuple<string, Sprite>("",null);
        if (texts.Count <= index)
            return new Tuple<string, Sprite>("",null);
        return new Tuple<string, Sprite>(texts[index].name,texts[index].face);
    }
    
    
    private int index = 0;
    public override bool isFinished => index >= texts.Count;
    
    protected override void StartDialogue(DialogueValue last)
    {
        index = texts.Count;
        DialogueInterface dialogueInterface = PersistenceDataScene.Instance.dialogueInterface;
        if (dialogueInterface == null)
            return;
        if (last == null)
        {
            dialogueInterface.ShowDialogueBox();
        }
        index = 0;
        dialogueInterface.dialogueText.text = GetText(0);
        Tuple<string,Sprite> character = GetCharacter(0);
        dialogueInterface.SetCharacterImage(character.Item2);
        //dialogueInterface.characterName.text = character.Item1;
    }
    
    protected override void UpdateDialogue()
    {
        if (InputPreset.current.actionInput.GetButtonDown())
        {
            index++;
            if (index >= texts.Count)
            {
                isFinished = true;
                return;
            }
            DialogueInterface dialogueInterface = PersistenceDataScene.Instance.dialogueInterface;
            if (dialogueInterface == null)
                return;
            dialogueInterface.dialogueText.text = GetText(index);
            Tuple<string,Sprite> character = GetCharacter(index);
            dialogueInterface.SetCharacterImage(character.Item2);
            //dialogueInterface.characterName.text = character.Item1;
        }
    }

    protected override void EndDialogue()
    {
        index = texts.Count;
        DialogueInterface dialogueInterface = PersistenceDataScene.Instance.dialogueInterface;
        if (dialogueInterface == null)
            return;
        if (next == null)
        {
            dialogueInterface.HideDialogueBox();
        }
    }

}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(DialogueTextValue))]
public class DialogueValueEditor : UnityEditor.Editor
{
    private LanguageSystemValue languageSystemValue = null;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        languageSystemValue = (LanguageSystemValue)AssetDatabase.LoadAssetAtPath("Assets/ScriptableObjects/Language System Value.asset", typeof(LanguageSystemValue));
        if (languageSystemValue == null)
        {
            if (GUILayout.Button("Create Language System Value"))
            {
                languageSystemValue = ScriptableObject.CreateInstance<LanguageSystemValue>();
                AssetDatabase.CreateAsset(languageSystemValue, "Assets/ScriptableObjects/Language System Value.asset");
                AssetDatabase.SaveAssets();
            }
            return;
        }
        DialogueTextValue dialogueValue = (DialogueTextValue)target;
        if (dialogueValue.texts == null)
        {
            dialogueValue.texts = new List<DialogueTextValue.Dialogue>();
        }
        if (GUILayout.Button("Add Text"))
        {
            dialogueValue.texts.Add(new DialogueTextValue.Dialogue());
        }
        //Spacing
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        //Check if number of languages is equal to number of texts
        for (int i = 0; i < dialogueValue.texts.Count; i++)
        {
            if (dialogueValue.texts==null)
                dialogueValue.texts = new List<DialogueTextValue.Dialogue>();
            while (dialogueValue.texts[i].text.Count < languageSystemValue.languagesName.Count)
            {
                dialogueValue.texts[i].text.Add("INITIAL_TEXT");
            }
            while (dialogueValue.texts[i].text.Count > languageSystemValue.languagesName.Count)
            {
                dialogueValue.texts[i].text.RemoveAt(dialogueValue.texts[i].text.Count - 1);
            }
        }
        
        //Show Text
        for (int i = 0; i < dialogueValue.texts.Count; i++)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text " + i);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                dialogueValue.texts.RemoveAt(i);
                return;
            }
            EditorGUILayout.EndHorizontal();
            //Draw Image
            dialogueValue.texts[i].face = (Sprite)EditorGUILayout.ObjectField("Face", dialogueValue.texts[i].face, typeof(Sprite), true);
            //Draw Name
            dialogueValue.texts[i].name = EditorGUILayout.TextField("Name", dialogueValue.texts[i].name);
            //Draw Text
            for (int j = 0; j < dialogueValue.texts[i].text.Count; j++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(languageSystemValue.languagesName[j]);
                dialogueValue.texts[i].text[j] = EditorGUILayout.TextArea(dialogueValue.texts[i].text[j]);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif