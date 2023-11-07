using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LoadSystemScene : MonoBehaviour
{
    public GameObject eventSystem;
    private void Awake()
    {
        if (PersistenceDataScene.Instance == null && UnityEngine.SceneManagement.SceneManager.GetSceneByName("System").isLoaded == false)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("System", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
    
    private void Start()
    {
        //keep only one EventSystem in the scene
        if (eventSystem != null && PersistenceDataScene.Instance.eventSystem != null)
        {
            Destroy(eventSystem);
            eventSystem = PersistenceDataScene.Instance.eventSystem;
        }
    }
}
