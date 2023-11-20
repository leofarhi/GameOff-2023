using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerZone : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public UnityEvent onTriggerStay;
    public string tagFilter = "Player";
    
    private void OnTriggerEnter(Collider other)
    {
        if (tagFilter != "" && !other.CompareTag(tagFilter)) return;
        onTriggerEnter.Invoke();
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (tagFilter != "" && !other.CompareTag(tagFilter)) return;
        onTriggerExit.Invoke();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (tagFilter != "" && !other.CompareTag(tagFilter)) return;
        onTriggerStay.Invoke();
    }
}
