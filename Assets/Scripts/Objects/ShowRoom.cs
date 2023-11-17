using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShowRoom : MonoBehaviour
{
    public MeshRenderer HideObject;
    [SerializeField]
    private bool showGizmos = true;
    private Material _material;

    public void Awake()
    {
        HideObject.enabled = true;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            //Clone le material pour ne pas modifier le material de base
            _material = Instantiate(meshRenderer.material);
            meshRenderer.material = _material;
            //set l'alpha du material à 1
            Color color = _material.color;
            color.a = 1f;
            _material.color = color;
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        Color color = Color.yellow;
        color.a = 0.5f;
        Gizmos.color = color;
        //dessine un cube à la position du trigger
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        Gizmos.matrix = transform.localToWorldMatrix;
        //calcul de la taille du cube et sa position en fonction du collider et du transform
        Vector3 size = Vector3.Scale(boxCollider.size, transform.localScale);
        Vector3 center = boxCollider.center;
        center.Scale(transform.localScale);
        Gizmos.DrawCube(center, size);
    }

    private void OnTriggerEnter(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();
        if (player != null)
        {
            //HideObject.enabled = false;
            StartCoroutine(Fade(_material.color.a, 0.0f, 0.5f));
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        ThirdPersonController player = other.GetComponent<ThirdPersonController>();
        if (player != null)
        {
            //HideObject.enabled = true;
            StartCoroutine(Fade(_material.color.a, 1.0f, 0.5f));
        }
    }
    
    //Smoothly changes the alpha value of the material
    IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / duration);
            _material.color = new Color(_material.color.r, _material.color.g, _material.color.b, alpha);
            yield return null;
        }
    }
}
