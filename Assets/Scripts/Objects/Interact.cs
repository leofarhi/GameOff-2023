using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interact : MonoBehaviour
{
    public enum DetectType
    {
        Distance,
        Trigger
    }
    
    public UnityEvent onInteract;
    public string tagFilter = "Player";
    public DetectType detectType;
    public float distance;
    public bool autoDisable = true;
    public bool waitExit = false;

    public bool isInteractable
    {
        get
        {
            return PlayerHUD.instance.interactionWith==this;
        }
        set
        {
            if (PlayerHUD.instance.interactionWith==null || PlayerHUD.instance.interactionWith==this)
                PlayerHUD.instance.interactionWith = value ? this : null;
        }
    }
    
    private void Update()
    {
        if (detectType == DetectType.Distance)
        {
            if (waitExit)
            {
                if (Vector3.Distance(transform.position, ThirdPersonController.instance.transform.position) < distance)
                    return;
                else
                {
                    waitExit = false;
                }
            }
            if (Vector3.Distance(transform.position, ThirdPersonController.instance.transform.position) < distance)
            {
                isInteractable = true;
            }
            else
            {
                if (isInteractable)
                    isInteractable = false;
            }
        }
        if (waitExit) return;
        if (InputPreset.current.actionInput.GetButtonDown() && isInteractable)
        {
            InteractWith();
        }
    }
    
    public void InteractWith()
    {

        if (isInteractable)
        {
            onInteract.Invoke();
            if (autoDisable)
            {
                isInteractable = false;
                waitExit = true;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (tagFilter != "" && !other.CompareTag(tagFilter)) return;
        if (detectType == DetectType.Trigger)
        {
            isInteractable = true;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (tagFilter != "" && !other.CompareTag(tagFilter)) return;
        if (detectType == DetectType.Trigger)
        {
            isInteractable = false;
            waitExit = false;
        }
    }
    
    public void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState != GameState.Gameplay)
        {
            isInteractable = false;
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
