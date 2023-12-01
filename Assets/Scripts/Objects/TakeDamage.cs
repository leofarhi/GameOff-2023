using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TakeDamage : MonoBehaviour
{
    public float damage = 10f;
    public GameObject owner;
    public UnityEvent OnHit;
    public void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();
        if (player != null)
        {
            player.TakeDamage(damage, owner);
            OnHit?.Invoke();
        }
    }
}
