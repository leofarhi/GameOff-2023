using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class AnimationObject : MonoBehaviour
{
    [Serializable]
    public class Action
    {
        public enum ActionType
        {
            UnityEvent,
            
            PlayAnimation,
            SetAnimatorBool,
            SetAnimatorTrigger,
            SetAnimatorFloat,
            SetAnimatorInt,
            
            StartDialogue,
            
            FmodSetParameter,
        }
        public ActionType actionType;
        public UnityEvent unityEvent;
        
        public Animator animator;
        public string animatorAttribute;
        public bool animatorBoolValue;
        public float animatorFloatValue;
        public int animatorIntValue;
        
        public DialogueValue dialogueValue;
        
        //constructor
        public Action()
        {
            unityEvent = new UnityEvent();
        }
        
        public void DoAction()
        {
            switch (actionType)
            {
                case ActionType.UnityEvent:
                    unityEvent.Invoke();
                    break;
                case ActionType.SetAnimatorBool:
                    animator.SetBool(animatorAttribute, animatorBoolValue);
                    break;
                case ActionType.SetAnimatorTrigger:
                    animator.SetTrigger(animatorAttribute);
                    break;
                case ActionType.SetAnimatorFloat:
                    animator.SetFloat(animatorAttribute, animatorFloatValue);
                    break;
                case ActionType.SetAnimatorInt:
                    animator.SetInteger(animatorAttribute, animatorIntValue);
                    break;
                case ActionType.PlayAnimation:
                    animator.Play(animatorAttribute);
                    break;
                case ActionType.StartDialogue:
                    PersistenceDataScene.Instance.LaunchDialogue(dialogueValue,unityEvent);
                    break;
                case ActionType.FmodSetParameter:
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName(animatorAttribute, animatorFloatValue);
                    break;
            }
        }
    }
    
    [Serializable]
    public class Animation
    {
        public string animationName;
        public List<Action> actions;
        
        //constructor
        public Animation()
        {
            actions = new List<Action>();
        }
        
        public void Play()
        {
            foreach (Action action in actions)
            {
                action.DoAction();
            }
        }
    }
    [HideInInspector]
    public List<Animation> animations;
    
    public void PlayAnimation(string animationName)
    {
        foreach (Animation animation in animations)
        {
            if (animation.animationName == animationName)
            {
                animation.Play();
                return;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimationObject))]
public class AnimationObjectEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AnimationObject animationObject = (AnimationObject)target;
        if (animationObject.animations == null)
        {
            animationObject.animations = new List<AnimationObject.Animation>();
        }
        if (GUILayout.Button("Add Animation"))
        {
            animationObject.animations.Add(new AnimationObject.Animation());
        }
        //Spacing
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        for (int i = 0; i < animationObject.animations.Count; i++)
        {
            AnimationObject.Animation animation = animationObject.animations[i];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            animation.animationName = EditorGUILayout.TextField("Animation Name", animation.animationName);
            if (GUILayout.Button("Remove"))
            {
                animationObject.animations.RemoveAt(i);
                return;
            }

            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add Action"))
            {
                animation.actions.Add(new AnimationObject.Action());
            }

            for (int j = 0; j < animation.actions.Count; j++)
            {
                AnimationObject.Action action = animation.actions[j];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                action.actionType =
                    (AnimationObject.Action.ActionType)EditorGUILayout.EnumPopup("Action Type", action.actionType);
                if (GUILayout.Button("Remove"))
                {
                    animation.actions.RemoveAt(j);
                    return;
                }

                EditorGUILayout.EndHorizontal();
                switch (action.actionType)
                {
                    case AnimationObject.Action.ActionType.UnityEvent:
                        // not work :EditorGUILayout.PropertyField(serializedObject.FindProperty("animations").GetArrayElementAtIndex(i).FindPropertyRelative("actions").GetArrayElementAtIndex(j).FindPropertyRelative("unityEvent"));
                        //Draw the UnityEvent
                        SerializedObject serializedObject = new SerializedObject(animationObject);
                        SerializedProperty property = serializedObject.FindProperty("animations").GetArrayElementAtIndex(i).FindPropertyRelative("actions").GetArrayElementAtIndex(j).FindPropertyRelative("unityEvent");
                        EditorGUILayout.PropertyField(property);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    case AnimationObject.Action.ActionType.SetAnimatorBool:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Animator Attribute", action.animatorAttribute);
                        action.animatorBoolValue =
                            EditorGUILayout.Toggle("Animator Bool Value", action.animatorBoolValue);
                        action.animator = (Animator) EditorGUILayout.ObjectField("Animator", action.animator,
                            typeof(Animator), true);
                        break;
                    case AnimationObject.Action.ActionType.SetAnimatorTrigger:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Animator Attribute", action.animatorAttribute);
                        action.animator = (Animator) EditorGUILayout.ObjectField("Animator", action.animator,
                            typeof(Animator), true);
                        break;
                    case AnimationObject.Action.ActionType.SetAnimatorFloat:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Animator Attribute", action.animatorAttribute);
                        action.animatorFloatValue =
                            EditorGUILayout.FloatField("Animator Float Value", action.animatorFloatValue);
                        action.animator = (Animator) EditorGUILayout.ObjectField("Animator", action.animator,
                            typeof(Animator), true);
                        break;
                    case AnimationObject.Action.ActionType.SetAnimatorInt:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Animator Attribute", action.animatorAttribute);
                        action.animatorIntValue =
                            EditorGUILayout.IntField("Animator Int Value", action.animatorIntValue);
                        action.animator = (Animator) EditorGUILayout.ObjectField("Animator", action.animator,
                            typeof(Animator), true);
                        break;
                    case AnimationObject.Action.ActionType.PlayAnimation:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Animator Attribute", action.animatorAttribute);
                        action.animator = (Animator) EditorGUILayout.ObjectField("Animator", action.animator,
                            typeof(Animator), true);
                        break;
                    case AnimationObject.Action.ActionType.StartDialogue:
                        action.dialogueValue = (DialogueValue) EditorGUILayout.ObjectField("Dialogue Value",
                            action.dialogueValue, typeof(DialogueValue), true);
                        SerializedObject serializedObject2 = new SerializedObject(animationObject);
                        SerializedProperty property2 = serializedObject2.FindProperty("animations").GetArrayElementAtIndex(i).FindPropertyRelative("actions").GetArrayElementAtIndex(j).FindPropertyRelative("unityEvent");
                        EditorGUILayout.PropertyField(property2, new GUIContent("On Dialogue End"));
                        serializedObject2.ApplyModifiedProperties();
                        break;
                    case AnimationObject.Action.ActionType.FmodSetParameter:
                        action.animatorAttribute =
                            EditorGUILayout.TextField("Fmod Parameter", action.animatorAttribute);
                        action.animatorFloatValue =
                            EditorGUILayout.FloatField("Fmod Parameter Value", action.animatorFloatValue);
                        break;
                }

                EditorGUILayout.EndVertical();
                //Spacing line
                EditorGUILayout.Space();
                //line
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }

            EditorGUILayout.EndVertical();
            //Spacing
            EditorGUILayout.Space();
            //Save
            EditorUtility.SetDirty(animationObject);
        }
    }
}
#endif
