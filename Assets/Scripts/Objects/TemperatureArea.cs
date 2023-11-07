using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TemperatureArea : MonoBehaviour
{
    public int temperature = 20;//in celsius
    
    void OnDrawGizmos()
    {
        //degradé de couleur en fonction de la température
        //bleu = froid
        //rouge = chaud
        Color color = Color.Lerp(Color.blue, Color.red, (temperature + 50) / 100f);
        color.a = 0.5f;
        Gizmos.color = color;
        //dessine un cube à la position du trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
        gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
        gameObject.GetComponent<BoxCollider>().size = Vector3.one;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        TemperatureBehaviour temperatureBehaviour = other.GetComponent<TemperatureBehaviour>();
        if (temperatureBehaviour != null)
        {
            temperatureBehaviour.temperatureAreas.Add(this);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        TemperatureBehaviour temperatureBehaviour = other.GetComponent<TemperatureBehaviour>();
        if (temperatureBehaviour != null && temperatureBehaviour.temperatureAreas.Contains(this))
        {
            temperatureBehaviour.temperatureAreas.Remove(this);
        }
    }
}
