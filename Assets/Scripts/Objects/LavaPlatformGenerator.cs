using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaPlatformGenerator : MonoBehaviour
{
    public GameObject platformPrefab;
    public float lifeTime = 5f;
    public int maxPlatforms = 5;
    List<GameObject> platforms = new List<GameObject>();

    public void Update()
    {
        if (platforms.Count>maxPlatforms)
        {
            GameObject platform = platforms[0];
            StartCoroutine(Dispawn(platform));
        }
    }

    public void OnTriggerEnter(Collider collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            if (projectile.temperatureAmount<0)
            {
                //get hit point
                Vector3 pos = collision.ClosestPointOnBounds(transform.position);
                Vector3 spawnPos = new Vector3(pos.x, platformPrefab.transform.position.y, pos.z);
                GameObject platform = Instantiate(platformPrefab,spawnPos, Quaternion.identity);
                platform.SetActive(true);
                platform.transform.parent = transform;
                platforms.Add(platform);
                StartCoroutine(LifeTime(platform));
            }
        }
    }
    
    public IEnumerator LifeTime(GameObject platform)
    {
        yield return Spawn(platform);
        yield return new WaitForSeconds(lifeTime);
        yield return Dispawn(platform);
    }
    
    public IEnumerator Spawn(GameObject platform)
    {
        float time = 0;
        Vector3 initialScale = platform.transform.localScale;
        while (time < 1f)
        {
            platform.transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, time);
            time += Time.deltaTime;
            yield return null;
        }
    }
    
    public IEnumerator Dispawn(GameObject platform)
    {
        if (!platforms.Contains(platform))
            yield break;
        platforms.Remove(platform);
        yield return Shake(platform);
        float time = 0;
        Vector3 initialScale = platform.transform.localScale;
        while (time < 1f)
        {
            platform.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, time);
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(platform);
    }
    
    public IEnumerator Shake(GameObject platform)
    {
        float time = 0;
        Vector3 initialPos = platform.transform.position;
        while (time < 1f)
        {
            platform.transform.position = initialPos + UnityEngine.Random.insideUnitSphere * 0.1f;
            time += Time.deltaTime;
            yield return null;
        }
    }
}
