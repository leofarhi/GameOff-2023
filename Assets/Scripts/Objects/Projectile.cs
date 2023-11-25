using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
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
    public SphereCollider collider;
    public GameObject owner;
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
    
    public IEnumerator Dispawn()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        float time = 0;
        Vector3 initialScale = gameObject.transform.localScale;
        while (time < 1f)
        {
            gameObject.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time);
            time += Time.deltaTime;
            yield return null;
        }
        OnDestroy?.Invoke();
        Destroy(gameObject);
    }
    
    public void FreeShoot(Vector3 direction)
    {
        OnFree?.Invoke();
        this.transform.parent = null;
        rigidbody.isKinematic = false;
        rigidbody.AddForce(direction * 10f, ForceMode.Impulse);
    }
}
