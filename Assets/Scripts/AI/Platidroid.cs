using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Platidroid : Enemy
{
    [Header("Color")]
    public Material colorMaterial;
    public SkinnedMeshRenderer meshRenderer;
    public Color hotColor;
    public Color coldColor;
    [Header("Death")]
    public GameObject coldDeathEffect;
    public GameObject hotDeathEffect;

    private int _animIDSpeed;
    private int _animIDMotionSpeed;
    private int _animIDLookAround;
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDLookAround = Animator.StringToHash("LookAround");
    }
    // Start is called before the first frame update
    void Start()
    {
        //Clone material to allow for color change
        //Find index of material
        int index = meshRenderer.materials.ToList().FindIndex(x => x.name.Contains(colorMaterial.name));
        //Clone material
        if (index >= 0)
        {
            meshRenderer.materials[index] = new Material(colorMaterial);
            colorMaterial = meshRenderer.materials[index];
        }
        else
        {
            Debug.LogError("Could not find material " + colorMaterial.name + " in " + gameObject.name);
        }
        InitState();
        AssignAnimationIDs();
    }
    
    public void UpdateColor()
    {
        if (colorMaterial == null)
        {
            Debug.LogError("Color material is null");
            return;
        }
        float t = Range(minTemperature, maxTemperature, currentTemperature);
        colorMaterial.color = Color.Lerp(coldColor, hotColor, t);
    }

    // Update is called once per frame
    public override void Update()
    {
        attackTarget = PersistenceDataScene.Instance.player.gameObject;
        if (currentTemperature>maxTemperature || currentTemperature<minTemperature)
        {
            Die();
        }
        base.Update();
        UpdateColor();
        if (agent.speed == 0)
        {
            agent.velocity = agent.velocity/2;
        }
        if (enemyState == EnemyState.Attack && attackCooldown > 0)
        {
            animator.SetFloat(_animIDSpeed, 0);
            animator.SetFloat(_animIDMotionSpeed, 1);
        }
        else
        {
            animator.SetFloat(_animIDSpeed, agent.speed * 2.5f);
            animator.SetFloat(_animIDMotionSpeed, Mathf.Clamp(agent.velocity.magnitude, 0, 1.5f));
        }
        animator.SetBool(_animIDLookAround, enemyState == EnemyState.IdlePatrol && agent.velocity.magnitude <= 0.05);
        UpdateSate();
    }

    public override IEnumerator AttackAnimation()
    {
        //Orient towards target
        Vector3 direction = attackTarget.transform.position - transform.position;
        direction.y = 0;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
        while (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(direction)) > 5)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.1f);
            yield return null;
        }
        //Play attack animation
        animator.PlayInFixedTime("SimpleAttack", 0, 0);
        yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("SimpleAttack"))
        {
            yield return null;
        }
        _attackCoroutine = null;
    }
    
    public override void Die()
    {
        base.Die();
        //distance of temperature
        float t = Range(minTemperature, maxTemperature, currentTemperature);
        //Spawn death effect
        if (t < 0.5f)
        {
            Instantiate(coldDeathEffect, transform.position, Quaternion.identity).SetActive(true);
        }
        else
        {
            Instantiate(hotDeathEffect, transform.position, Quaternion.identity).SetActive(true);
        }
        Destroy(gameObject);
    }
}
