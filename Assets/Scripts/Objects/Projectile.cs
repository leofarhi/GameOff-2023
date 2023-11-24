using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Projectile : MonoBehaviour
{
    [Serializable]
    public class ProjectilesPrefab
    {
        public GameObject frostProjectile;
        public GameObject fireProjectile;
        
        public GameObject get(TemperatureBehaviour.TemperatureState state)
        {
            if (state == TemperatureBehaviour.TemperatureState.Cold)
            {
                return frostProjectile;
            }
            else if (state == TemperatureBehaviour.TemperatureState.Hot)
            {
                return fireProjectile;
            }
            return null;
        }
    }

    public Rigidbody rigidbody;
    public UnityEvent OnSpawn;
    public UnityEvent OnFree;
    public UnityEvent OnHit;
    public UnityEvent OnDestroy;
    // Start is called before the first frame update
    void Start()
    {
        OnSpawn?.Invoke();
        if (rigidbody == null)
        {
            rigidbody = GetComponent<Rigidbody>();
        }
        rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void FreeShoot(Vector3 direction)
    {
        OnFree?.Invoke();
        this.transform.parent = null;
        rigidbody.isKinematic = false;
        rigidbody.AddForce(direction * 10f, ForceMode.Impulse);
    }
}
