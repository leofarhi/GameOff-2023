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
    public LayerMask wallLayer;
    public GameObject owner;
    public float temperatureAmount = 10f;
    public float lifeTime = 5f;
    public List<Light> lights = new List<Light>();
    public UnityEvent OnSpawn;
    public UnityEvent OnFree;
    public UnityEvent OnHit;
    public UnityEvent OnDestroy;
    private IEnumerator dispawnCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        if (lights==null)
        {
            lights = new List<Light>();
        }
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
    
    public void Dispawn()
    {
        if (dispawnCoroutine != null)
        {
            return;
        }
        dispawnCoroutine = _dispawn();
        StartCoroutine(dispawnCoroutine);
    }
    
    private IEnumerator _dispawn()
    {
        rigidbody.isKinematic = true;
        collider.enabled = false;
        float time = 0;
        Vector3 initialScale = gameObject.transform.localScale;
        Tuple<float,float>[] rangeAndIntensity = new Tuple<float, float>[lights.Count];
        for (int i = 0; i < lights.Count; i++)
        {
            rangeAndIntensity[i] = new Tuple<float, float>(lights[i].range, lights[i].intensity);
        }
        while (time < 1f)
        {
            gameObject.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time);
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].range = Mathf.Lerp(rangeAndIntensity[i].Item1, 0, time);
                lights[i].intensity = Mathf.Lerp(rangeAndIntensity[i].Item2, 0, time);
            }
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
        StartCoroutine(LifeTime());
    }
    
    private IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        Dispawn();
    }
    
    public void SetOwner(GameObject owner,Collider collider=null)
    {
        this.owner = owner;
        if (collider == null)
        {
            collider = owner.GetComponent<Collider>();
        }
        if (collider != null)
        {
            Physics.IgnoreCollision(this.collider, collider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TemperatureBehaviour temperatureBehaviour = other.GetComponent<TemperatureBehaviour>();
        if (temperatureBehaviour != null
            && temperatureBehaviour.gameObject!=owner
            && GameStateManager.Instance.CurrentGameState == GameState.Gameplay)
        {
            temperatureBehaviour.TakeDamageTemperature(temperatureAmount);
            OnHit?.Invoke();
            Dispawn();
        }
        /*if (LayerMask.LayerToName(other.gameObject.layer)== "GroundCollider")
        {
            Dispawn();
        }*/
        //wallLayer
        if (wallLayer == (wallLayer | (1 << other.gameObject.layer)))
        {
            Dispawn();
        }
    }
}
